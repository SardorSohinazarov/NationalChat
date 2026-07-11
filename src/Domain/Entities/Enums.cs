namespace Domain.Entities;

public enum ChatType
{
    Private,
    Group,
    Channel,
    Secret
}

public enum ChatMemberRole
{
    Member,
    Admin,
    Creator
}

public enum PollType
{
    Regular,
    Quiz,
    MultipleChoice
}

public enum CallType
{
    Audio,
    Video
}

public enum AttachmentType
{
    Photo,
    Video,
    File,
    Sticker
}

public enum SubscriptionStatus
{
    Active,
    Canceled,
    Expired,
    Pending
}