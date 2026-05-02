export const LoginRequestDTO = {
    Email: "string",
    Password: "string",
}

export const LoginResponseDTO = {
    id: "string",
    username: "string",
    avatarUrl: "string | null",
    accessToken: "string",
    refreshToken: "string",
}
