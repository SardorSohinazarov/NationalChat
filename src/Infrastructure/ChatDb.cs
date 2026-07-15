using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class ChatDb : DbContext
{
    public ChatDb(DbContextOptions<ChatDb> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<ChatMember> ChatMembers => Set<ChatMember>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Poll> Polls => Set<Poll>();
    public DbSet<PollOption> PollOptions => Set<PollOption>();
    public DbSet<PollVote> PollVotes => Set<PollVote>();
    public DbSet<Call> Calls => Set<Call>();
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Sticker> Stickers => Set<Sticker>();
    public DbSet<StickerSet> StickerSets => Set<StickerSet>();
    public DbSet<FolderChat> FolderChats => Set<FolderChat>();
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Story> Stories => Set<Story>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<EmailVerificationCode> EmailVerificationCodes => Set<EmailVerificationCode>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
    public DbSet<BlockedUser> BlockedUsers => Set<BlockedUser>();
    public DbSet<ChannelSubscriber> ChannelSubscribers => Set<ChannelSubscriber>();
    public DbSet<SavedMessage> SavedMessages => Set<SavedMessage>();
    public DbSet<Bot> Bots => Set<Bot>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<SecretChat> SecretChats => Set<SecretChat>();
    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<CallParticipant> CallParticipants => Set<CallParticipant>();
    public DbSet<StoryView> StoryViews => Set<StoryView>();
    public DbSet<MessageView> MessageViews => Set<MessageView>();
    public DbSet<TwoFactorAuth> TwoFactorAuths => Set<TwoFactorAuth>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatDb).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
