<template>
  <h1 class="disply-4">Quartz.Net Dashboard</h1>

  <div class="container">
    <div class="col-md-12">
      <div id="css_table" style="width: 100%">
        <div class="css_tr css_title">
          <div class="css_td">Group</div>
          <div class="css_td">任務名稱</div>
          <div class="css_td">Start time</div>
          <div class="css_td">上次執行時間</div>
          <div class="css_td">下次執行時間</div>
          <div class="css_td">End Time</div>
          <div class="css_td">狀態</div>
          <div class="css_td">執行計劃</div>
          <div class="css_td">任務描述</div>
          <div class="css_td"></div>
          <div class="css_td"></div>
          <div class="css_td"></div>
          <div class="css_td"></div>
        </div>
        <div class="css_tr" v-for="item in jobs" :key="item">
          <div class="css_td">{{ item.jobGroup }}</div>
          <div class="css_td">{{ item.jobName }}</div>
          <div class="css_td">{{ formatDateTime(item.startTime) }}</div>
          <div class="css_td">{{ formatDateTime(item.previousFireTime) }}</div>
          <div class="css_td">{{ formatDateTime(item.nextFireTime) }}</div>
          <div class="css_td">{{ item.endTime }}</div>
          <div class="css_td">{{ item.displayState }}</div>
          <div class="css_td">{{ item.scheduleExpression }}</div>
          <div class="css_td">{{ item.scheduleExpressionDesc }}</div>
          <div class="css_td myMOUSE" style="text-align: center" title="暫停" @click="suspendJob(item.jobGroup, item.jobName)">
            <i class="fas fa-pause"></i>
          </div>
          <div class="css_td" style="text-align: center" title="恢復執行" @click="resumeJob(item.jobGroup, item.jobName)">
            <i class="fas fa-reply"></i>
          </div>
          <div class="css_td" style="text-align: center" title="立即執行" @click="immediatelyExecuteJob(item.jobGroup, item.jobName)">
            <i class="fas fa-play"></i>
          </div>
          <div class="css_td" style="text-align: center" title="刪除" @click="deleteJob(item.jobGroup, item.jobName)">
            <i class="fas fa-trash"></i>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from "vue";
import myHub from "@/libs/myHub";
import { datetimeISOFormat } from "@/libs/utils";

export default {
  setup() {
    const jobs = ref([]); //資料集, 放置 Job 資料

    // 查詢參數
    const queryParam = ref({
      group: "", // Group
      jobName: "" // job 名稱
    });

    // 取得 Job 資料集
    const getAllJobs = async () => {
      if (myHub.state.toString() !== "Connected") {
        console.log("SignalR 還未連線");
        await myHub.start();
      }

      try {
        const result = await myHub.invoke("getAllJobs"); // call signalr method
        console.log("getAllJobs result", result.data);
        jobs.value = result.data;
      } catch (er) {
        console.log(`取得所有 Job 資料錯誤 getAllJobs error!--[${er.toString()}]`);
      }
    };

    // 暫停 Job 的執行, 呼叫 SuspendJob
    const suspendJob = async (jobGroupName, jobName) => {
      if (myHub.state.toString() != "Connected") {
        console.log("SignalR 還未連線");
        await myHub.start();
      }

      try {
        const result = await myHub.invoke("SuspendJob", jobName, jobGroupName); // call signalr method
        console.log("SuspendJob result", result);
        await getAllJobs();
      } catch (er) {
        console.log("SuspendJob error", er.toString());
      }
    };

    //恢復 Job 的執行
    const resumeJob = async (jobGroupName, jobName) => {
      if (myHub.state.toString() != "Connected") {
        console.log("SignalR 還未連線");
        await myHub.start();
      }

      try {
        const result = await myHub.invoke("resumeJob", jobName, jobGroupName); // call signalr method
        console.log("resumeJob result", result);
        await getAllJobs();
      } catch (er) {
        console.log("resumeJob error", er.toString());
      }
    };

    //立即執行
    const immediatelyExecuteJob = async (jobGroupName, jobName) => {
      if (myHub.state.toString() != "Connected") {
        console.log("SignalR 還未連線");
        await myHub.start();
      }

      myHub
        .invoke("immediatelyExecuteJob", jobName, jobGroupName) // call signalr method
        .then(function (rtnValue) {
          console.log("immediatelyExecuteJob rtnValue", rtnValue);
          getAllJobs();
        })
        .catch(function (err) {
          console.log("immediatelyExecuteJob error", err.toString());
        });
    };

    //刪除 Job
    const deleteJob = async (jobGroupName, jobName) => {
      if (myHub.state.toString() != "Connected") {
        console.log("SignalR 還未連線");
        await myHub.start();
      }

      try {
        const result = await myHub.invoke("deleteJob", jobName, jobGroupName); // call signalr method
        console.log("deleteJob result", result);

        if (result.code === "200") {
          await getAllJobs();
        } else {
          let msg = `刪除 Job 失敗!\ncode: ${result.code}\nmessage:${result.message}`;
          alert(msg);
        }
      } catch (er) {
        console.log("deleteJob error", er.toString());
      }
    };

    // 提供 SignalR Hub 呼叫的方法
    myHub.on("JobStatusChange", () => {
      console.log("JobStatusChange 被觸發");
      getAllJobs();
    });

    onMounted(() => {
      getAllJobs();
    });

    // 格式化日期
    const formatDateTime = (dateValue) => {
      return datetimeISOFormat(dateValue);
    };

    return {
      jobs, //物件: 回傳 job 資料集
      queryParam, //物件: 查詢參數
      formatDateTime, //方法: 格式化日期
      suspendJob, //方法:暫停 Job
      resumeJob, //方法:恢復 Job
      immediatelyExecuteJob, //方法:立即執行
      deleteJob //方法:刪除 Job
    };
  }
};
</script>

<style></style>
