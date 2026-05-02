const regexUtils = {
    USERNAME: /^[a-zA-Z][a-zA-Z0-9-_#]{4,24}$/,
    PASSWORD: /^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,64}$/,
    EMAIL: /^[^\s@]+@[^\s@]+\.[^\s@]+$/
}

export default regexUtils;