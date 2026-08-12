using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "messaging");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.EnsureSchema(
                name: "bot");

            migrationBuilder.EnsureSchema(
                name: "call");

            migrationBuilder.EnsureSchema(
                name: "chat");

            migrationBuilder.EnsureSchema(
                name: "security");

            migrationBuilder.EnsureSchema(
                name: "storage");

            migrationBuilder.EnsureSchema(
                name: "personal");

            migrationBuilder.EnsureSchema(
                name: "story");

            migrationBuilder.CreateTable(
                name: "chats",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false, comment: "1 = Private, 2 = Group, 3 = Channel, 4 = Secret"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "email_verification_codes",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Purpose = table.Column<int>(type: "integer", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConsumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequestIpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_verification_codes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "files",
                schema: "storage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<int>(type: "integer", nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_files", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "channels",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    InviteLink = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SignatureEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_channels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_channels_chats_ChatId",
                        column: x => x.ChatId,
                        principalSchema: "chat",
                        principalTable: "chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "photos",
                schema: "storage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileId = table.Column<int>(type: "integer", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_photos_files_FileId",
                        column: x => x.FileId,
                        principalSchema: "storage",
                        principalTable: "files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Bio = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ProfilePhotoId = table.Column<int>(type: "integer", nullable: true),
                    IsProfileCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_photos_ProfilePhotoId",
                        column: x => x.ProfilePhotoId,
                        principalSchema: "storage",
                        principalTable: "photos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "blocked_users",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    BlockedUserId = table.Column<int>(type: "integer", nullable: false),
                    BlockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blocked_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_blocked_users_users_BlockedUserId",
                        column: x => x.BlockedUserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_blocked_users_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bots",
                schema: "bot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Commands = table.Column<string>(type: "TEXT", nullable: true),
                    CanJoinGroups = table.Column<bool>(type: "boolean", nullable: false),
                    InlineFeedback = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bots_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "calls",
                schema: "call",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HostId = table.Column<int>(type: "integer", nullable: false),
                    ChatId = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false, comment: "1 = Audio, 2 = Video")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_calls_chats_ChatId",
                        column: x => x.ChatId,
                        principalSchema: "chat",
                        principalTable: "chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_calls_users_HostId",
                        column: x => x.HostId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "channel_subscribers",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_channel_subscribers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_channel_subscribers_channels_ChannelId",
                        column: x => x.ChannelId,
                        principalSchema: "chat",
                        principalTable: "channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_channel_subscribers_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "chat_members",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false, comment: "1 = Member, 2 = Admin, 3 = Creator"),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_chat_members_chats_ChatId",
                        column: x => x.ChatId,
                        principalSchema: "chat",
                        principalTable: "chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_chat_members_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contacts",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ContactUserId = table.Column<int>(type: "integer", nullable: false),
                    CustomFirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomLastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_contacts_users_ContactUserId",
                        column: x => x.ContactUserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_contacts_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "folders",
                schema: "personal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_folders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_folders_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "groups",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    InviteLink = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatorId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_groups_chats_ChatId",
                        column: x => x.ChatId,
                        principalSchema: "chat",
                        principalTable: "chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_groups_users_CreatorId",
                        column: x => x.CreatorId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatId = table.Column<int>(type: "integer", nullable: false),
                    SenderId = table.Column<int>(type: "integer", nullable: false),
                    TextContent = table.Column<string>(type: "TEXT", nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplyToMessageId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_messages_chats_ChatId",
                        column: x => x.ChatId,
                        principalSchema: "chat",
                        principalTable: "chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_messages_messages_ReplyToMessageId",
                        column: x => x.ReplyToMessageId,
                        principalSchema: "messaging",
                        principalTable: "messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_messages_users_SenderId",
                        column: x => x.SenderId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "secret_chats",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InitiatorId = table.Column<int>(type: "integer", nullable: false),
                    ParticipantId = table.Column<int>(type: "integer", nullable: false),
                    EncryptionKey = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_secret_chats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_secret_chats_users_InitiatorId",
                        column: x => x.InitiatorId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_secret_chats_users_ParticipantId",
                        column: x => x.ParticipantId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SystemVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AppVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    RefreshTokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastActiveAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sessions_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sticker_sets",
                schema: "storage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatorId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsAnimated = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sticker_sets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sticker_sets_users_CreatorId",
                        column: x => x.CreatorId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stories",
                schema: "story",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FileId = table.Column<int>(type: "integer", nullable: false),
                    Caption = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stories_files_FileId",
                        column: x => x.FileId,
                        principalSchema: "storage",
                        principalTable: "files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_stories_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                schema: "personal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "1 = Active, 2 = Canceled, 3 = Expired, 4 = Pending"),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PricePaid = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_subscriptions_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "two_factor_auth",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Hint = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RecoveryEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_two_factor_auth", x => x.Id);
                    table.ForeignKey(
                        name: "FK_two_factor_auth_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "call_participants",
                schema: "call",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CallId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_call_participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_call_participants_calls_CallId",
                        column: x => x.CallId,
                        principalSchema: "call",
                        principalTable: "calls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_call_participants_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "folder_chats",
                schema: "personal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FolderId = table.Column<int>(type: "integer", nullable: false),
                    ChatId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_folder_chats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_folder_chats_chats_ChatId",
                        column: x => x.ChatId,
                        principalSchema: "chat",
                        principalTable: "chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_folder_chats_folders_FolderId",
                        column: x => x.FolderId,
                        principalSchema: "personal",
                        principalTable: "folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "attachments",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<int>(type: "integer", nullable: false),
                    FileId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false, comment: "1 = Photo, 2 = Video, 3 = File, 4 = Sticker")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_attachments_files_FileId",
                        column: x => x.FileId,
                        principalSchema: "storage",
                        principalTable: "files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_attachments_messages_MessageId",
                        column: x => x.MessageId,
                        principalSchema: "messaging",
                        principalTable: "messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "message_views",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_views", x => x.Id);
                    table.ForeignKey(
                        name: "FK_message_views_messages_MessageId",
                        column: x => x.MessageId,
                        principalSchema: "messaging",
                        principalTable: "messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_message_views_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "polls",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<int>(type: "integer", nullable: false),
                    Question = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsAnonymous = table.Column<bool>(type: "boolean", nullable: false),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false, comment: "1 = Regular, 2 = Quiz, 3 = MultipleChoice")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_polls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_polls_messages_MessageId",
                        column: x => x.MessageId,
                        principalSchema: "messaging",
                        principalTable: "messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reactions",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Emoji = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ReactedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reactions_messages_MessageId",
                        column: x => x.MessageId,
                        principalSchema: "messaging",
                        principalTable: "messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reactions_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "saved_messages",
                schema: "personal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MessageId = table.Column<int>(type: "integer", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saved_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_saved_messages_messages_MessageId",
                        column: x => x.MessageId,
                        principalSchema: "messaging",
                        principalTable: "messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_saved_messages_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stickers",
                schema: "storage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SetId = table.Column<int>(type: "integer", nullable: false),
                    Emoji = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FileId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stickers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stickers_files_FileId",
                        column: x => x.FileId,
                        principalSchema: "storage",
                        principalTable: "files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_stickers_sticker_sets_SetId",
                        column: x => x.SetId,
                        principalSchema: "storage",
                        principalTable: "sticker_sets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "story_views",
                schema: "story",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StoryId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_story_views", x => x.Id);
                    table.ForeignKey(
                        name: "FK_story_views_stories_StoryId",
                        column: x => x.StoryId,
                        principalSchema: "story",
                        principalTable: "stories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_story_views_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "poll_options",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PollId = table.Column<int>(type: "integer", nullable: false),
                    TextVal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poll_options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_poll_options_polls_PollId",
                        column: x => x.PollId,
                        principalSchema: "messaging",
                        principalTable: "polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "poll_votes",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PollId = table.Column<int>(type: "integer", nullable: false),
                    OptionId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    VotedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poll_votes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_poll_votes_poll_options_OptionId",
                        column: x => x.OptionId,
                        principalSchema: "messaging",
                        principalTable: "poll_options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_poll_votes_polls_PollId",
                        column: x => x.PollId,
                        principalSchema: "messaging",
                        principalTable: "polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_poll_votes_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attachments_FileId",
                schema: "messaging",
                table: "attachments",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_attachments_MessageId",
                schema: "messaging",
                table: "attachments",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_blocked_users_BlockedUserId",
                schema: "identity",
                table: "blocked_users",
                column: "BlockedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_blocked_users_UserId",
                schema: "identity",
                table: "blocked_users",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_bots_UserId",
                schema: "bot",
                table: "bots",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_call_participants_CallId",
                schema: "call",
                table: "call_participants",
                column: "CallId");

            migrationBuilder.CreateIndex(
                name: "IX_call_participants_UserId",
                schema: "call",
                table: "call_participants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_calls_ChatId",
                schema: "call",
                table: "calls",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_calls_HostId",
                schema: "call",
                table: "calls",
                column: "HostId");

            migrationBuilder.CreateIndex(
                name: "IX_channel_subscribers_ChannelId",
                schema: "chat",
                table: "channel_subscribers",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_channel_subscribers_UserId",
                schema: "chat",
                table: "channel_subscribers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_channels_ChatId",
                schema: "chat",
                table: "channels",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_chat_members_ChatId",
                schema: "chat",
                table: "chat_members",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_chat_members_UserId",
                schema: "chat",
                table: "chat_members",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_contacts_ContactUserId",
                schema: "identity",
                table: "contacts",
                column: "ContactUserId");

            migrationBuilder.CreateIndex(
                name: "IX_contacts_UserId",
                schema: "identity",
                table: "contacts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_email_verification_codes_Email_Purpose_ExpiresAt",
                schema: "security",
                table: "email_verification_codes",
                columns: new[] { "Email", "Purpose", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_folder_chats_ChatId",
                schema: "personal",
                table: "folder_chats",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_folder_chats_FolderId",
                schema: "personal",
                table: "folder_chats",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_folders_UserId",
                schema: "personal",
                table: "folders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_groups_ChatId",
                schema: "chat",
                table: "groups",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_groups_CreatorId",
                schema: "chat",
                table: "groups",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_message_views_MessageId",
                schema: "messaging",
                table: "message_views",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_message_views_UserId",
                schema: "messaging",
                table: "message_views",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_messages_ChatId",
                schema: "messaging",
                table: "messages",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_messages_ReplyToMessageId",
                schema: "messaging",
                table: "messages",
                column: "ReplyToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_messages_SenderId",
                schema: "messaging",
                table: "messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_photos_FileId",
                schema: "storage",
                table: "photos",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_poll_options_PollId",
                schema: "messaging",
                table: "poll_options",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_poll_votes_OptionId",
                schema: "messaging",
                table: "poll_votes",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_poll_votes_PollId",
                schema: "messaging",
                table: "poll_votes",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_poll_votes_UserId",
                schema: "messaging",
                table: "poll_votes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_polls_MessageId",
                schema: "messaging",
                table: "polls",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_reactions_MessageId",
                schema: "messaging",
                table: "reactions",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_reactions_UserId",
                schema: "messaging",
                table: "reactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_saved_messages_MessageId",
                schema: "personal",
                table: "saved_messages",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_saved_messages_UserId",
                schema: "personal",
                table: "saved_messages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_secret_chats_InitiatorId",
                schema: "chat",
                table: "secret_chats",
                column: "InitiatorId");

            migrationBuilder.CreateIndex(
                name: "IX_secret_chats_ParticipantId",
                schema: "chat",
                table: "secret_chats",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_RefreshTokenHash",
                schema: "security",
                table: "sessions",
                column: "RefreshTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sessions_UserId_RevokedAt_ExpiresAt",
                schema: "security",
                table: "sessions",
                columns: new[] { "UserId", "RevokedAt", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_sticker_sets_CreatorId",
                schema: "storage",
                table: "sticker_sets",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_stickers_FileId",
                schema: "storage",
                table: "stickers",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_stickers_SetId",
                schema: "storage",
                table: "stickers",
                column: "SetId");

            migrationBuilder.CreateIndex(
                name: "IX_stories_FileId",
                schema: "story",
                table: "stories",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_stories_UserId",
                schema: "story",
                table: "stories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_story_views_StoryId",
                schema: "story",
                table: "story_views",
                column: "StoryId");

            migrationBuilder.CreateIndex(
                name: "IX_story_views_UserId",
                schema: "story",
                table: "story_views",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_UserId",
                schema: "personal",
                table: "subscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_two_factor_auth_UserId",
                schema: "security",
                table: "two_factor_auth",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                schema: "identity",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_ProfilePhotoId",
                schema: "identity",
                table: "users",
                column: "ProfilePhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Username",
                schema: "identity",
                table: "users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attachments",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "blocked_users",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "bots",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "call_participants",
                schema: "call");

            migrationBuilder.DropTable(
                name: "channel_subscribers",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "chat_members",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "contacts",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "email_verification_codes",
                schema: "security");

            migrationBuilder.DropTable(
                name: "folder_chats",
                schema: "personal");

            migrationBuilder.DropTable(
                name: "groups",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "message_views",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "poll_votes",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "reactions",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "saved_messages",
                schema: "personal");

            migrationBuilder.DropTable(
                name: "secret_chats",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "sessions",
                schema: "security");

            migrationBuilder.DropTable(
                name: "stickers",
                schema: "storage");

            migrationBuilder.DropTable(
                name: "story_views",
                schema: "story");

            migrationBuilder.DropTable(
                name: "subscriptions",
                schema: "personal");

            migrationBuilder.DropTable(
                name: "two_factor_auth",
                schema: "security");

            migrationBuilder.DropTable(
                name: "calls",
                schema: "call");

            migrationBuilder.DropTable(
                name: "channels",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "folders",
                schema: "personal");

            migrationBuilder.DropTable(
                name: "poll_options",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "sticker_sets",
                schema: "storage");

            migrationBuilder.DropTable(
                name: "stories",
                schema: "story");

            migrationBuilder.DropTable(
                name: "polls",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "messages",
                schema: "messaging");

            migrationBuilder.DropTable(
                name: "chats",
                schema: "chat");

            migrationBuilder.DropTable(
                name: "users",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "photos",
                schema: "storage");

            migrationBuilder.DropTable(
                name: "files",
                schema: "storage");
        }
    }
}
