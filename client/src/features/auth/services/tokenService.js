import authAxios from "../../../api/authAxios";
import APIs from "../../../api/ApiURL";
import { mapRefeshTokenResponce } from "../mappers/refreshTokenMapper";

export const tokenService = {
    refresh: async (data) => {
        const res = await authAxios.post(
            APIs.REFRESH_TOKEN,
            JSON.stringify(data),
        );
        return mapRefeshTokenResponce(res.data);
    },

    save: (refreshToken) => {
        sessionStorage.setItem("refreshToken", refreshToken);
    },

    load: () => (
        sessionStorage.getItem("refreshToken")
    ),

    clear: () => {
        sessionStorage.removeItem("refreshToken");
    },

    isExpired: (refreshToken) => {
        if (!refreshToken) return true;
        try {
            const decoded = JSON.parse(atob(refreshToken.split('.')[1]));
            return decoded.exp * 1000 < Date.now();
        } catch {
            return true;
        }
    }
}