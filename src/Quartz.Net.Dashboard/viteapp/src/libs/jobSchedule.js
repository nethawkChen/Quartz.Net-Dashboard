import req from "@/libs/axiosApi";

// 取得所有 Job 排程
export const queryJobs = queryObj => {
  console.log("jobSchedule.js", "queryJobs called, queryObj:", queryObj);
  return req("post", "/JobsManger/GetJobSchedules", queryObj);
};

// 新增 Job 排程
export const createJob = queryObj => {
  console.log("jobSchedule.js", "createJob called, queryObj:", queryObj);
  return req("post", "/JobsManger/Create", queryObj);
};

// 更新 Job 排程
export const updateJob = queryObj => {
  console.log("jobSchedule.js", "updateJob called, queryObj:", queryObj);
  return req("post", "/JobsManger/Update", queryObj);
};

// 刪除 Job 排程
export const deleteJob = queryObj => {
  console.log("jobSchedule.js", "deleteJob called, queryObj:", queryObj);
  return req("post", "/JobsManger/Delete", queryObj);
};
