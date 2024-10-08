﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace PricePoint.API.UnitOfWork
{
    /// <summary>
    /// Defines the interface(s) for generic unit of work.
    /// </summary>
    public interface IUnitOfWork<TContext, TUser, TRole> : IUnitOfWork<TUser, TRole> 
        where TContext : DbContext 
        where TUser : IdentityUser<Guid> 
        where TRole : IdentityRole<Guid>
    {
        /// <summary>
        /// Gets the db context.
        /// </summary>
        /// <returns>The instance of type <typeparamref name="TContext"/>.</returns>
        TContext DbContext { get; }

        /// <summary>
        /// Saves all changes made in this context to the database with distributed transaction.
        /// </summary>
        /// <param name="ensureAutoHistory"><c>True</c> if save changes ensure auto record the change history.</param>
        /// <param name="unitOfWorks">An optional <see cref="IUnitOfWork{TUser, TRole}"/> array.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous save operation. The task result contains the number of state entities written to database.</returns>
        Task<int> SaveChangesAsync(bool ensureAutoHistory = false, params IUnitOfWork<TUser, TRole>[] unitOfWorks);

    }

    /// <summary>
    /// Defines the interface(s) for unit of work.
    /// </summary>
    public interface IUnitOfWork<TUser, TRole> : IDisposable 
        where TUser : class 
        where TRole : class
    {
        /// <summary>
        /// Changes the database name. This require the databases in the same machine. NOTE: This only work for MySQL right now.
        /// </summary>
        /// <param name="database">The database name.</param>
        /// <remarks>
        /// This only been used for supporting multiple databases in the same model. This require the databases in the same machine.
        /// </remarks>
        //void ChangeDatabase(string database);

        /// <summary>
        /// Gets the specified repository for the <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="hasCustomRepository"><c>True</c> if providing custom repository</param>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>An instance of type inherited from <see cref="IRepository{TEntity}"/> interface.</returns>
        IRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = false) where TEntity : class;

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <param name="ensureAutoHistory"><c>True</c> if save changes ensure auto record the change history.</param>
        /// <returns>The number of state entries written to the database.</returns>
        int SaveChanges(bool ensureAutoHistory = false);

        /// <summary>
        /// Asynchronously saves all changes made in this unit of work to the database.
        /// </summary>
        /// <param name="ensureAutoHistory"><c>True</c> if save changes ensure auto record the change history.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous save operation. The task result contains the number of state entities written to database.</returns>
        Task<int> SaveChangesAsync(bool ensureAutoHistory = false);

        /// <summary>
        /// Executes the specified raw SQL command.
        /// </summary>
        /// <param name="sql">The raw SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The number of state entities written to database.</returns>
        //int ExecuteSqlCommand(string sql, params object[] parameters);

        /// <summary>
        /// Uses raw SQL queries to fetch the specified <typeparamref name="TEntity"/> data.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="sql">The raw SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>An <see cref="IQueryable{T}"/> that contains elements that satisfy the condition specified by raw SQL.</returns>
        //IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class;

        /// <summary>
        /// Uses Track Graph Api to attach disconnected entities
        /// </summary>
        /// <param name="rootEntity"> Root entity</param>
        /// <param name="callback">Delegate to convert Object's State properties to Entities entry state.</param>
        void TrackGraph(object rootEntity, Action<EntityEntryGraphNode> callback);

        /// <summary>
        /// Returns UserManager
        /// </summary>
        /// <returns></returns>
        UserManager<TUser> GetUserManager();

        /// <summary>
        /// Returns RoleManager
        /// </summary>
        /// <returns></returns>
        RoleManager<TRole> GetRoleManager();

        /// <summary>
        /// Returns Transaction 
        /// </summary>
        /// <returns></returns>
        Task<IDbContextTransaction> BeginTransactionAsync(bool useIfExists = false);

        /// <summary>
        /// Returns Transaction 
        /// </summary>
        /// <returns></returns>
        IDbContextTransaction BeginTransaction(bool useIfExists = false);

        /// <summary>
        /// DbContext disable/enable auto detect changes
        /// </summary>
        /// <param name="value"></param>
        void SetAutoDetectChanges(bool value);

        /// <summary>
        /// Last error after save changes fired
        /// </summary>
        SaveChangesResult LastSaveChangesResult { get; }
    }
}