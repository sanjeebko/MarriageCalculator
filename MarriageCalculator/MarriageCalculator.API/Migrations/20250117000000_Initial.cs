using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarriageCalculator.API.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if tables already exist (since database schema is already correct)
            // Only create tables if they don't exist
            
            // User table
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='User' AND xtype='U')
                BEGIN
                    CREATE TABLE [User] (
                        [Id] [uniqueidentifier] NOT NULL,
                        [DisplayName] [nvarchar](100) NOT NULL,
                        [Email] [nvarchar](255) NOT NULL,
                        [PasswordHash] [nvarchar](max) NOT NULL,
                        [Salt] [nvarchar](max) NOT NULL,
                        [IsEmailVerified] [bit] NOT NULL,
                        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
                        [LastLoginAt] [datetime2](7) NULL,
                        [IsActive] [bit] NOT NULL,
                        CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([Id] ASC)
                    );
                    CREATE UNIQUE NONCLUSTERED INDEX [IX_User_Email] ON [User] ([Email] ASC);
                END
            ");

            // UserEmailVerification table
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserEmailVerification' AND xtype='U')
                BEGIN
                    CREATE TABLE [UserEmailVerification] (
                        [Id] [int] IDENTITY(1,1) NOT NULL,
                        [UserId] [uniqueidentifier] NOT NULL,
                        [VerificationCode] [nvarchar](5) NOT NULL,
                        [ExpiresAt] [datetime2](7) NOT NULL,
                        [IsUsed] [bit] NOT NULL DEFAULT 0,
                        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
                        [UsedAt] [datetime2](7) NULL,
                        CONSTRAINT [PK_UserEmailVerification] PRIMARY KEY CLUSTERED ([Id] ASC),
                        CONSTRAINT [FK_UserEmailVerification_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
                    );
                    CREATE NONCLUSTERED INDEX [IX_UserEmailVerification_UserId_VerificationCode_IsUsed] ON [UserEmailVerification] ([UserId] ASC, [VerificationCode] ASC, [IsUsed] ASC);
                END
            ");

            // RefreshToken table
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RefreshToken' AND xtype='U')
                BEGIN
                    CREATE TABLE [RefreshToken] (
                        [Id] [int] IDENTITY(1,1) NOT NULL,
                        [UserId] [uniqueidentifier] NOT NULL,
                        [Token] [nvarchar](256) NOT NULL,
                        [ExpiresAt] [datetime2](7) NOT NULL,
                        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
                        [IsActive] [bit] NOT NULL,
                        [RevokedAt] [datetime2](7) NULL,
                        [ReplacedByToken] [nvarchar](256) NULL,
                        [RevokedReason] [nvarchar](100) NULL,
                        CONSTRAINT [PK_RefreshToken] PRIMARY KEY CLUSTERED ([Id] ASC),
                        CONSTRAINT [FK_RefreshToken_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
                    );
                    CREATE UNIQUE NONCLUSTERED INDEX [IX_RefreshToken_Token] ON [RefreshToken] ([Token] ASC);
                    CREATE NONCLUSTERED INDEX [IX_RefreshToken_UserId_IsActive] ON [RefreshToken] ([UserId] ASC, [IsActive] ASC);
                END
            ");

            // Note: Other tables (GameSettings, Player, etc.) are handled by existing migrations
            // This migration only ensures the User authentication tables are properly set up
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("RefreshToken");
            migrationBuilder.DropTable("UserEmailVerification");
            migrationBuilder.DropTable("User");
        }
    }
}