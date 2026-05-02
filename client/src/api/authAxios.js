import axios from "axios";
import APIs from "./ApiURL";

export default axios.create({
  baseURL: APIs.SERVER_URL,
  withCredentials: true,
  headers: { 'Content-Type': 'application/json' }
});
