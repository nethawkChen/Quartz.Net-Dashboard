import aios from "axios";

const instance = aios.create({
  baseURL: import.meta.env.VITE_API_LOCAL
});

instance.interceptors.request.use(
  config => {
    return config;
  },
  error => {
    return Promise.reject(error);
  }
);

instance.interceptors.response.use(
  response => {
    return response;
  },
  error => {
    return Promise.reject(error);
  }
);

export default function (method, url, data = null, config) {
  method = method.toLowerCase();
  switch (method) {
    case "post":
      return instance.post(url, data, config);
    case "get":
      return instance.get(url, { params: data });
    case "put":
      return instance.put(url, data);
    case "delete":
      return instance.delete(url, { params: data });
    case "patch":
      return instance.patch(url, data);
    default:
      console.log(`不支援的 method: ${method}`);
      return false;
  }
}
