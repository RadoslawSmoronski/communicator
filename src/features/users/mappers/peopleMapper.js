export const mapUserDtoToPerson = (dto) => ({
    id: dto.id,
    username: dto.username,
    avatarUrl: dto.avatarUrl,
    isInvited: dto.isInvited
})

export const mapPeopleList = (dtoList) =>
    dtoList.map(mapUserDtoToPerson);