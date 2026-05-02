export const mapUserToSessionStorage = (dto) => ({
    userID: dto.userID,
    username: dto.username,
    email: dto.email,
    avatarUrl: dto.avatarUrl,
    role: dto.role
});