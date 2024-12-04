import { AuthService } from '@/services/AuthService';
import { useCommonStore } from '@/stores';
import { inject } from 'vue';

export const SessionManager = {
  getSettings: async function () {
    const commonStore = useCommonStore();
    const authService = inject<AuthService>('authService');

    if (commonStore.userInfo) {
      return true;
    }

    try {
      const userInfo = await authService?.getUserInfo();
      if (!userInfo) {
        console.error('User info not available.');
        return false;
      }
      commonStore.userInfo = userInfo;
      return true;
    } catch (error) {
      console.log(error);
      return false;
    }
  },
};

export const splunkLog = (message) => {
  // TODO: This has to be refactored to use a better way to call Splunk via REST API
  console.log(message);
  // const token = import.meta.env["SPLUNK_TOKEN"] || ""
  // const url = import.meta.env["SPLUNK_COLLECTOR_URL"] || ""

  // if (token && url) {
  //   const config = {
  //     token: token,
  //     url: url
  //   }

  //   const Logger = new SplunkLogger(config)
  //   const payload = {
  //     message: message
  //   }

  //   Logger.send(payload, (err, resp, body) => {
  //     console.log("Response from Splunk", body)
  //   })
  // }
};

export const getSingleValue = (value: string | string[]): string => {
  return Array.isArray(value) ? value[0] : value;
};
