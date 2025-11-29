export const mapLoginResponseToUser = (dto, email) => ({
    userID: dto.id,
    username: dto.username,
    email: email,
    avatarUrl: dto.avatarUrl,
    role: 'user',
    currentChat: '',
    accessToken: dto.accessToken,
    refreshToken: dto.refreshToken
});