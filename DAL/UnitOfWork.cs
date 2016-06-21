﻿using Riipen_SSD.DAL.Repositories.Concrete_Implementations;
using Riipen_SSD.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Riipen_SSD.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SSD_RiipenEntities _context;

        public IContestRepository Contests { get; private set; }
        public ICriteriaScoreRepository CriteriaScores { get; private set; }
        public ICriterionRepository Criteria { get; private set; }
        public IFeedbackRepository Feedback { get; private set; }
        public ITeamRepository Teams { get; private set; }
        public IAspNetUserRepository Users { get; private set; }
        public IAspNetRoleRepository Roles { get; private set; }
        public IContestJudgeRepository ContestJudges { get; private set; }

        public UnitOfWork(SSD_RiipenEntities context)
        {
            _context = context;

            Contests = new ContestRepository(_context);
            CriteriaScores = new CriteriaScoreRepository(_context);
            Criteria = new CriterionRepository(_context);
            Feedback = new FeedbackRepository(_context);
            Teams = new TeamRepository(_context);
            Users = new AspNetUserRepository(_context);
            Roles = new AspNetRoleRepository(_context);
            ContestJudges = new ContestJudgeRepository(_context);
        }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}