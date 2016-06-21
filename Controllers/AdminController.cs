﻿using Riipen_SSD.AdminViewModels;
using Riipen_SSD.DAL;
using Riipen_SSD.ExtensionMethods;
using Riipen_SSD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using Riipen_SSD.BusinessLogic;
using Riipen_SSD.ViewModels.AdminViewModels;
using System.Threading.Tasks;

namespace Riipen_SSD.Controllers
{

    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationUserManager UserManager;
        private IUnitOfWork UnitOfWork;

        public AdminController(ApplicationUserManager userManager, IUnitOfWork unitOfWork)
        {
            UserManager = userManager;
            UnitOfWork = unitOfWork;
        }
        // GET: Admin
        public ActionResult Index(String searchAContest, String sortContests, int? page)
        {
            IEnumerable<AdminViewModels.IndexContestVM> adminContests = UnitOfWork.Contests.GetAll().Select(x => new AdminViewModels.IndexContestVM() { Name = x.Name, StartTime = x.StartTime.ToString(), Location = x.Location, Published = x.Published, ContestID = x.Id });
            string searchStringValue = "";
            string sortStringValue = "Latest Contests";

            // if search input is not null or empty
            if (!String.IsNullOrEmpty(searchAContest))
            {
                adminContests = adminContests.Where(a => a.Name.ToUpper().Contains(searchAContest.ToUpper()));

                searchStringValue = searchAContest;
            }

            if (!String.IsNullOrEmpty(sortContests))
            {
                if (sortContests == "Name")
                {
                    adminContests = adminContests.OrderBy(a => a.Name).ThenBy(a => a.StartTime);
                    sortStringValue = "Name";

                }
                else if (sortContests == "Location")
                {
                    adminContests = adminContests.OrderBy(a => a.Location).ThenBy(a => a.StartTime);
                    sortStringValue = "Location";
                }
            }
            else
            {
                adminContests = adminContests.OrderBy(a => a.StartTime);
            }


            ViewBag.SearchStringValue = searchStringValue;
            ViewBag.SortStringValue = sortStringValue;

            const int PAGE_SIZE = 10;
            int pageNumber = (page ?? 1);
            IEnumerable<AdminViewModels.IndexContestVM> newAdminContests = adminContests;
            newAdminContests = newAdminContests.ToPagedList(pageNumber, PAGE_SIZE);

            return View(newAdminContests);
        }

        [HttpGet]
        public ActionResult ContestDetails(int? contestID)
        {

            if (contestID != null)
            {
                var contest = UnitOfWork.Contests.Get(contestID.Value);
                if (contest == null)
                {
                    return RedirectToAction("Index", "Admin");
                }
                var judges = contest.ContestJudges.Select(x => new JudgeVM()
                {
                    FirstName = x.AspNetUser.FirstName,
                    LastName = x.AspNetUser.LastName,
                    Email = x.AspNetUser.Email
                });
                var participants = new List<ContestDetailsParticipantVM>();
                foreach (var team in contest.Teams)
                {
                    var participantsFromTeam = team.AspNetUsers.Select(x => new ContestDetailsParticipantVM() { Email = x.Email, FirstName = x.FirstName, LastName = x.LastName, TeamName = team.Name, RiipenUrl = x.RiipenUrl });
                    participants.AddRange(participantsFromTeam);
                }

                var contestDetailVM = new ContestDetailsVM()
                {
                    ContestID = contestID.Value,
                    Name = contest.Name,
                    StartTime = contest.StartTime.ToString(),
                    Location = contest.Location,
                    Published = contest.Published,
                    Participants = participants,
                    Judges = judges,
                };

                ViewBag.ContestID = contestID;

                return View(contestDetailVM);
            }
            else
            {
                return RedirectToAction("Index", "admin");
            }


        }
        [HttpPost]
        public async Task<ActionResult> SendEmails(int? contestID)
        {

            if (contestID != null)
            {
                //on "publish" click, get list of judges and participants for this contest

                var contest = UnitOfWork.Contests.Get(contestID.Value);
                contest.Published = true;
                UnitOfWork.Complete();
                var judges = contest.ContestJudges.Select(x => new JudgeVM()
                {
                    FirstName = x.AspNetUser.FirstName,
                    LastName = x.AspNetUser.LastName,
                    Email = x.AspNetUser.Email
                });
                var participants = new List<ContestDetailsParticipantVM>();
                foreach (var team in contest.Teams)
                {
                    var participantsFromTeam = team.AspNetUsers.Select(x => new ContestDetailsParticipantVM() { Email = x.Email, FirstName = x.FirstName, TeamName = team.Name });
                    participants.AddRange(participantsFromTeam);
                }

                var contestDetailVM = new ContestDetailsVM()
                {
                    ContestID = contestID.Value,
                    Name = contest.Name,
                    StartTime = contest.StartTime.ToString(),
                    Location = contest.Location,
                    Published = true,
                    Participants = participants,
                    Judges = judges,
                };
                foreach(var judge in judges)
                {

                    var getUser = UnitOfWork.Users.SingleOrDefault(x => x.Email == judge.Email);

                    var getID = getUser.Id;
                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(getID);

                    //var code = UserManager.GeneratePasswordResetTokenAsync(getID);
                    var callbackUrl = Url.Action("SetPassword", "Account", new { userID = getID, code = code }, protocol: Request.Url.Scheme);
                    MailHelper mailer = new MailHelper();
                    string subject = "Your Riipen account";
                    string body = "";
                    if (getUser.EmailConfirmed)
                    {
                        body = "Please log in to view your contest: <a href=\"http:\\riipen.whatthefelix.com\" >Log in </a>";
                    }
                    else
                    {
                        body = "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\"> Confirm </a>";
                    }
                    string response = mailer.EmailFromArvixe(new Message(judge.Email, subject, body));
                }

                foreach (var participant in contestDetailVM.Participants)
                {
                    var getUser = UnitOfWork.Users.SingleOrDefault(x => x.Email == participant.Email);

                    var getID = getUser.Id;
                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(getID);

                    //var code = UserManager.GeneratePasswordResetTokenAsync(getID);
                    var callbackUrl = Url.Action("SetPassword", "Account", new { userID = getID, code = code }, protocol: Request.Url.Scheme);
                    MailHelper mailer = new MailHelper();
                    string subject = "Your Riipen account";
                    string body = "";
                    if (getUser.EmailConfirmed)
                    {
                        body = "Please log in to view your contest: <a href=\"http:\\riipen.whatthefelix.com\" >Log in </a>";
                    }
                    else
                    {
                        body = "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\"> Confirm </a>";
                    }
                    string response = mailer.EmailFromArvixe(new Message(participant.Email, subject, body));
                }
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }

        }

        [HttpGet]
        public ActionResult CreateContest()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateContest(CreateContestVM contestVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            // THIS SEEMS TO WORK BUT NEEDS SOME REFACTORING 
            var contest = new Contest()
            {
                StartTime = contestVM.StartTime,
                Location = contestVM.Location,
                Name = contestVM.ContestName,
                Published = false,
                PubliclyViewable = false,
            };

            // for each participant, check if they have a user account - create one if they don't
            if (contestVM.Participants != null)
            {
                foreach (var participantVM in contestVM.Participants)
                {
                    var participantUser = UnitOfWork.Users.SingleOrDefault(x => x.Email == participantVM.Email);
                    if (participantUser == null)
                    {
                        var participantApplicationUser = new ApplicationUser { UserName = participantVM.Email, Email = participantVM.Email, FirstName = participantVM.FirstName, LastName = participantVM.LastName };
                        participantUser = AutoMapper.Mapper.Map<ApplicationUser, AspNetUser>(participantApplicationUser);
                        UnitOfWork.Users.Add(participantUser);
                    }
                    // for each participant, add them to a team - create the team if it doesn't exist already
                    var team = contest.Teams.FirstOrDefault(t => t.Name == participantVM.TeamName);
                    if (team == null)
                    {
                        team = new Team() { Name = participantVM.TeamName };
                        contest.Teams.Add(team);
                    }
                    team.AspNetUsers.Add(participantUser);

                }
            }

            // for each participant, check if they have a user account - create one if they don't - then add them to contest judges
            if (contestVM.Judges != null)
            {
                foreach (var judge in contestVM.Judges)
                {
                    var judgeUser = UnitOfWork.Users.SingleOrDefault(x => x.Email == judge.Email);
                    if (judgeUser == null)
                    {
                        var judgeApplicationUser = new ApplicationUser { UserName = judge.Email, Email = judge.Email, FirstName = judge.FirstName, LastName = judge.LastName };
                        
                        judgeUser = AutoMapper.Mapper.Map<ApplicationUser, AspNetUser>(judgeApplicationUser);
                        var judgeRole = UnitOfWork.Roles.SingleOrDefault(x => x.Name == "Judge");
                        judgeUser.AspNetRoles.Add(judgeRole);
                        UnitOfWork.Users.Add(judgeUser);
                    }
                    contest.ContestJudges.Add(new ContestJudge { AspNetUser = judgeUser });
                }
            }

            // add criteria
            contest.Criteria.AddRange(contestVM.Criteria.Select(x => new Criterion() { Description = x.Description, Name = x.Name }));

            contest = UnitOfWork.Contests.Add(contest);
            UnitOfWork.Complete();

            return RedirectToAction("ContestDetails", "Admin", new { contestID = contest.Id });
        }

        public ActionResult DeleteContest(int? contestID)
        {
            if (contestID.HasValue == false)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }

            var contest = UnitOfWork.Contests.SingleOrDefault(x => x.Id == contestID.Value);

            if (contest == null || contest.Published == true)
            {
                return Redirect(Request.UrlReferrer.ToString());
            }

            UnitOfWork.Criteria.RemoveRange(contest.Criteria);
            var teams = contest.Teams;
            foreach (var team in teams)
            {
                team.AspNetUsers.Clear();
            }
            UnitOfWork.Teams.RemoveRange(contest.Teams);
            UnitOfWork.ContestJudges.RemoveRange(contest.ContestJudges);

            UnitOfWork.Contests.Remove(contest);

            UnitOfWork.Complete();

            return RedirectToAction("Index");
        }

        //Participants = contest.Teams.SelectMany(x => x.AspNetUsers.Select(y => new ParticipantVM() { Email = y.Email, FirstName = y.FirstName, LastName = y.LastName, TeamName = x.Name })),

        public ActionResult ContestScores(int? contestID, String searchATeam, String sortTeams)
        {
            SSD_RiipenEntities context = new SSD_RiipenEntities();
            string searchStringValue = "";
            string sortStringValue = "Status";
            List<TeamScoreVM> TeamScoreList = new List<TeamScoreVM>();

            //get the number of team in this contest
            List<Team> teams = UnitOfWork.Contests.Get(contestID.Value).Teams.ToList();

            if (!String.IsNullOrEmpty(searchATeam))
            {
                teams = teams.Where(t => t.Name.ToUpper().Contains(searchATeam.ToUpper())).ToList();
                searchStringValue = searchATeam;
            }

            //get judge number for a contest
            int judgesNumber = UnitOfWork.ContestJudges.Find(cj => cj.ContestId == contestID).Count();

            //check if this team has final score or not
            foreach (var team in teams)
            {
                double? FinalScore = null;

                //get the number of submitted judges of the contest              
                List<CriteriaScore> getSubmitJudges = context.CriteriaScores.Where(ci => ci.ContestId == contestID && ci.TeamId == team.Id && ci.Submitted).GroupBy(x => x.Judge_ID).Select(x => x.FirstOrDefault()).ToList();

                //if all the judges has submitted
                if (judgesNumber == getSubmitJudges.Count())
                {
                    //get final score for a team
                    List<CriteriaScore> getFinalCriteriaScoreForATeam = context.CriteriaScores.Where(ci => ci.ContestId == contestID && ci.TeamId == team.Id).ToList();

                    if (getFinalCriteriaScoreForATeam.Count() != 0)
                    {
                        FinalScore = Math.Round((double)getFinalCriteriaScoreForATeam.Average(g => g.Score), 2);
                    }
                }

                TeamScoreList.Add(new TeamScoreVM(team.Id, team.Name, FinalScore));
            }

            if (sortTeams == "Name")
            {
                TeamScoreList = TeamScoreList.OrderBy(t => t.TeamName).ToList();
                sortStringValue = "Name";
            }
            else
            {
                TeamScoreList = TeamScoreList.OrderByDescending(t => t.FinalScore).ThenBy(t => t.TeamName).ToList();
            }

            ViewBag.searchStringValue = searchStringValue;
            ViewBag.sortStringValue = sortStringValue;
            ViewBag.contestName = UnitOfWork.Contests.Get(contestID.Value).Name;
            ViewBag.contestId = contestID;


            return View(TeamScoreList);

        }


        //pick a winner
        [HttpPost]
        public ActionResult PickWinner(int? FirstId, int? SecondId, int? ThirdId)
        {
            if (FirstId != null && SecondId != null && ThirdId != null)
            {
                Contest contest = UnitOfWork.Teams.Get((int)FirstId).Contest;
                contest.WinnerTeamId = FirstId;
                contest.SecondTeamId = SecondId;
                contest.ThirdTeamId = ThirdId;
                UnitOfWork.Complete();
            }

            return RedirectToAction("Index", "Admin");
        }
    }
}