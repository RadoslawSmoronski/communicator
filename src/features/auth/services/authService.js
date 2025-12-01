import axios from "../../../api/axios";
import APIs from "../../../api/ApiURL";
import { mapLoginResponseToUser } from "../mappers/loginMapper";

export const loginUser = async (data) => {
    const response = await axios.post(
        APIs.LOGIN,
        JSON.stringify(data)
    );

    return mapLoginResponseToUser(response.data, data.Email);
};
