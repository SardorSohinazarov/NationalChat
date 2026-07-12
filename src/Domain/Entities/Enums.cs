namespace Domain.Entities;

public enum ChatType
{
    Private = 1,
    Group = 2,
    Channel = 3,
    Secret = 4
}

public enum ChatMemberRole
{
    Member = 1,
    Admin = 2,
    Creator = 3
}

public enum PollType
{
    Regular = 1,
    Quiz = 2,
    MultipleChoice = 3
}

public enum CallType
{
    Audio = 1,
    Video = 2
}

public enum AttachmentType
{
    Photo = 1,
    Video = 2,
    File = 3,
    Sticker = 4
}

public enum SubscriptionStatus
{
    Active = 1,
    Canceled = 2,
    Expired = 3,
    Pending = 4
}