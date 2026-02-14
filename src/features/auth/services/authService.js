import authAxios from "../../../api/authAxios";
import APIs from "../../../api/ApiURL";
import { mapLoginResponseToUser } from "../mappers/loginMapper";

export const loginUser = async (data) => {
    const response = await authAxios.post(
        APIs.LOGIN,
        data
    );

    return mapLoginResponseToUser(response.data, data.Email);
};
