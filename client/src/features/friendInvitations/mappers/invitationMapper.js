export const mapInvitationDtoToInvitation = (dto) => ({
    id: dto.friendInvitationId,
    senderId: dto.senderId,
    username: dto.senderUsername,
    avatarUrl: dto.senderAvatarUrl,
});

export const mapInvitationList = (dtoList) =>
    dtoList.map(mapInvitationDtoToInvitation);
