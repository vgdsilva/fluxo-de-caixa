import { doPostRequest, doGetRequest } from "@/services/requestHelper";

let relativePath = "/auth/";

export default {
  /** payload: ContainerLogin ({ login, password }) */
  sAuthorize(payload) {
    return doPostRequest({ path: `${relativePath}login`, payload, skipAuth: true });
  },
  sLogout(refreshToken) {
    return doPostRequest({ path: `${relativePath}logout`, payload: { refreshToken }, skipAuth: true });
  },
  sMe() {
    return doGetRequest({ path: "/me" });
  },
};
