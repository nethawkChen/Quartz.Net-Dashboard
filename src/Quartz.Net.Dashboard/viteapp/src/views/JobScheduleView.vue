<template>
  <h1 class="display-4">任務管理</h1>

  <div class="container">
    <div class="col-md-12">
      <div id="css_table" style="background-color: dimgray; color: white; width: 100%">
        <div class="css_tr css_tb_title">
          <div class="css_td" style="width: 95%">設定 Job Schedule</div>
          <div class="css_td myMOUSE" style="text-align: center" title="新增" v-on:click="curdAction('add')">
            <i class="fas fa-plus"></i>
          </div>
        </div>
      </div>

      <div id="css_table" style="width: 100%">
        <div class="css_tr css_title">
          <div class="css_td">ID</div>
          <div class="css_td">JobName</div>
          <div class="css_td">Group</div>
          <div class="css_td">類型</div>
          <div class="css_td">Job描述</div>
          <div class="css_td">Cron表達弍</div>
          <div class="css_td">Schedule 描述</div>
          <div class="css_td">狀態</div>
          <div class="css_td"></div>
          <div class="css_td"></div>
        </div>
        <div class="css_tr" v-for="item in objDataLst" :key="item.ID">
          <div class="css_td">{{ item.id }}</div>
          <div class="css_td">{{ item.jobName }}</div>
          <div class="css_td">{{ item.jobGroup }}</div>
          <div class="css_td">{{ item.jobTypeName }}</div>
          <div class="css_td">{{ item.jobDesc }}</div>
          <div class="css_td">{{ item.scheduleExpression }}</div>
          <div class="css_td">{{ item.scheduleExpressionDesc }}</div>
          <div class="css_td">{{ item.jobStatus }}</div>
          <div class="css_td myMOUSE" v-on:click="curdAction('edit', item.id, item.jobName, item.jobGroup, item.jobTypeName, item.jobDesc, item.scheduleExpression, item.scheduleExpressionDesc, item.jobStatus)">
            <i class="fas fa-edit"></i>
          </div>
          <div class="css_td myMOUSE" v-on:click="deleteData(item.id, item.jobName,item.jobGroup)">
            <i class="fas fa-trash-alt"></i>
          </div>
        </div>
      </div>
    </div>

    <curd-box v-bind:isdisplay="lightboxShow" :errMsg="errMsg">
      <template v-slot:header>
        <span class="h5" style="font-weight: bold">{{ actionObj.title }}</span>
      </template>

      <div id="css_table">
        <div class="css_tr">
          <div class="css_td css_title">Id</div>
          <div class="css_td"><input type="text" v-model="tbModel.Id" readonly="readonly" style="background-color: darkgray" /></div>
        </div>
        <div class="css_tr">
          <div class="css_td css_title">任務名稱</div>
          <div class="css_td"><input type="text" v-model="tbModel.JobName" style="width: 100%" /></div>
        </div>
        <div class="css_tr">
          <div class="css_td css_title">任務群組</div>
          <div class="css_td"><input type="text" v-model="tbModel.JobGroup" style="width: 100%" /></div>
        </div>
        <div class="css_tr">
          <div class="css_td css_title">任務類別(Class name)</div>
          <div class="css_td"><input type="text" v-model="tbModel.JobTypeName" style="width: 100%" /></div>
        </div>
        <div class="css_tr">
          <div class="css_td css_title">任務敍述</div>
          <div class="css_td"><input type="text" v-model="tbModel.JobDesc" style="width: 100%" /></div>
        </div>
        <div class="css_tr">
          <div class="css_td css_title">排程表達式</div>
          <div class="css_td"><input type="text" v-model="tbModel.ScheduleExpression" style="width: 100%" /></div>
        </div>
        <div class="css_tr">
          <div class="css_td css_title">排程敍述</div>
          <div class="css_td"><input type="text" v-model="tbModel.ScheduleExpressionDesc" style="width: 100%" /></div>
        </div>
        <div class="css_tr">
          <div class="css_td css_title">狀態</div>
          <div class="css_td">
            <select v-model="tbModel.JobStatus">
              <option></option>
              <option value="Y">啟用</option>
              <option value="P">暫停</option>
              <option value="N">停用</option>
            </select>
          </div>
        </div>
      </div>

      <template v-slot:footer>
        <div style="text-align: center">
          <button id="btnSave" class="btn btn-primary" v-on:click="saveIt" v-text="actionObj.btnText"></button>
          &nbsp;
          <button id="btnCancel" class="btn btn-secondary" v-on:click="lightboxShow = !lightboxShow">取消</button>
        </div>
        <br />
      </template>
    </curd-box>
  </div>
</template>

<script>
import { ref, onMounted } from "vue";
import { queryJobs, createJob, updateJob, deleteJob } from "@/libs/jobSchedule.js";
import CurdBox from "@/components/CURDLightBox.vue";

export default {
  components: {
    CurdBox
  },
  setup() {
    const objDataLst = ref(null); //資料集
    const lightboxShow = ref(false); //是否顯示lightbox
    const errMsg = ref("");

    //新增﹑異動 對應的資料
    const tbModel = ref({
      Id: null,
      JobName: "",
      JobGroup: "",
      JobTypeName: "",
      JobDesc: "",
      JobData: "",
      ScheduleExpression: "",
      ScheduleExpressionDesc: "",
      JobStatus: ""
    });

    //異動前舊資料
    const old_tbModel = ref({
      Id: null,
      JobName: "",
      JobGroup: "",
      JobTypeName: "",
      JobDesc: "",
      JobData: "",
      ScheduleExpression: "",
      ScheduleExpressionDesc: "",
      JobStatus: ""
    });

    //動作物件
    const actionObj = ref({
      name: "", //動作名稱(add, edit, del)
      title: "", //標題
      btnText: "" //button 顯示文字
    });

    //新增﹑異動儲存
    const saveIt = () => {
      console.log("saveIt 被觸發");
      if (actionObj.value.name === "add") {
        insData();
      } else if (actionObj.value.name === "edit") {
        updData();
      }
    };

    //新增﹑異動
    const curdAction = (actionName, id, jobName, jobGroup, jobTypeName, jobDesc, scheduleExpression, scheduleExpressionDesc, jobStatus) => {
      lightboxShow.value = !lightboxShow.value;
      actionObj.value.name = actionName;

      if (actionName === "add") {
        actionObj.value.title = "新增任務資料";
        actionObj.value.btnText = "建立";

        tbModel.value.Id = "";
        tbModel.value.JobName = "";
        tbModel.value.JobGroup = "";
        tbModel.value.JobTypeName = "";
        tbModel.value.JobDesc = "";
        tbModel.value.ScheduleExpression = "";
        tbModel.value.ScheduleExpressionDesc = "";
        tbModel.value.JobStatus = "";

        console.log("add tbModel", tbModel);
      } else if (actionName === "edit") {
        actionObj.value.title = "編輯任務資料";
        actionObj.value.btnText = "儲存";

        tbModel.value.Id = id;
        tbModel.value.JobName = jobName;
        tbModel.value.JobGroup = jobGroup;
        tbModel.value.JobTypeName = jobTypeName;
        tbModel.value.JobDesc = jobDesc;
        tbModel.value.ScheduleExpression = scheduleExpression;
        tbModel.value.ScheduleExpressionDesc = scheduleExpressionDesc;
        tbModel.value.JobStatus = jobStatus;

        old_tbModel.value.Id = id;
        old_tbModel.value.JobName = jobName;
        old_tbModel.value.JobGroup = jobGroup;
        old_tbModel.value.JobTypeName = jobTypeName;
        old_tbModel.value.JobDesc = jobDesc;
        old_tbModel.value.ScheduleExpression = scheduleExpression;
        old_tbModel.value.ScheduleExpressionDesc = scheduleExpressionDesc;
        old_tbModel.value.JobStatus = jobStatus;

        console.log("edit tbModel", tbModel);
      }
      console.log("actionObj", actionObj);
      return lightboxShow.value;
    };

    //新增
    function insData() {
      console.log("外層 insData 被觸發");
      if (tbModel.value.JobName === "") {
        alert("必須輸入一個唯一的任務名稱!");
        return false;
      } else if (tbModel.value.jobTypeName === "") {
        alert("必須輸入任務的類型名稱!");
        return false;
      } else if (tbModel.value.scheduleExpression === "") {
        alert("必須輸入任務的排程表!");
        return false;
      }

      //要寫入的資料
      const jobDto = ref({
        jobName: tbModel.value.JobName,
        jobGroup: tbModel.value.JobGroup,
        jobTypeName: tbModel.value.JobTypeName,
        jobDesc: tbModel.value.JobDesc,
        scheduleExpression: tbModel.value.ScheduleExpression,
        scheduleExpressionDesc: tbModel.value.ScheduleExpressionDesc,
        jobStatus: tbModel.value.JobStatus
      });

      createJob(jobDto.value)
        .then(resp => {
          console.log("create job", resp);
          console.log("create job data", resp.data);

          if (resp.data.code === "200") {
            lightboxShow.value = false; //關閉燈箱
            m_queryObjData();
          } else {
            errMsg.value = "新增 Job 失敗!" + resp.data.message;
          }
        })
        .catch(error => {
          console.log("create job error", error);
          errMsg.value = "新增 Job 失敗!" + error;
        });
    }

    //異動
    function updData() {
      console.log("外層 updData 被觸發");
      if (tbModel.value.JobName === "") {
        alert("必須輸入一個唯一的任務名稱!");
        return false;
      } else if (tbModel.value.jobTypeName === "") {
        alert("必須輸入任務的類型名稱!");
        return false;
      } else if (tbModel.value.scheduleExpression === "") {
        alert("必須輸入任務的排程表!");
        return false;
      }

      //要異動的資料
      const jobDto = ref({
        id: tbModel.value.Id,
        jobName: tbModel.value.JobName,
        jobGroup: tbModel.value.JobGroup,
        jobTypeName: tbModel.value.JobTypeName,
        jobDesc: tbModel.value.JobDesc,
        scheduleExpression: tbModel.value.ScheduleExpression,
        scheduleExpressionDesc: tbModel.value.ScheduleExpressionDesc,
        jobStatus: tbModel.value.JobStatus
      });

      const updData = ref({
        newData: jobDto.value,
        oldData: old_tbModel.value
      });

      updateJob(updData.value)
        .then(resp => {
          console.log("update job", resp);
          console.log("update job data", resp.data);

          if (resp.data.code === "200") {
            lightboxShow.value = false; //關閉燈箱
            m_queryObjData();
          } else {
            errMsg.value = "更新 Job 失敗!" + resp.data.message;
          }
        })
        .catch(error => {
          console.log("update job error", error);
          errMsg.value = "更新 Job 失敗!" + error;
        });
    }

    // 刪除資料
    function deleteData(id, jobName, jobGroup) {
      console.log("外層 deleteData 被觸發");
      if (!confirm(`確定要刪除[任務:${jobName}(${id})] 此筆紀錄嗎?`)) {
        return false;
      }

      const jobDto = ref({
        id: id,
        jobName: jobName,
        jobGroup: jobGroup,
      });

      deleteJob(jobDto.value)
        .then(resp => {
          console.log("delete job", resp);
          console.log("delete job data", resp.data);

          //如果回傳的代碼是 200 表示刪除成功,如果不是則訊息帶刪除失敗
          if (resp.data.code === "200") {
            lightboxShow.value = false; //關閉燈箱
            m_queryObjData();
          } else {
            errMsg.value = "刪除 Job 失敗!" + resp.data.message;
          }
        })
        .catch(error => {
          console.log("delete job error", error);
          console.log("error message:", error.response.data.message);
          errMsg.value = "刪除 Job 失敗!" + error.response.data.message;
        });
    }

    //查詢資料
    function m_queryObjData() {
      queryJobs()
        .then(resp => {
          console.log("resp", resp);
          objDataLst.value = resp.data.datas;
        })
        .catch(er => {
          console.log("查詢異常:", er);
          errMsg.value = er.toString();
        });
    }

    onMounted(() => {
      m_queryObjData();
    });

    return {
      lightboxShow, //變數: 是否顯示lightbox
      errMsg, //物件: 錯誤訊息
      objDataLst, //物件: 資料集
      tbModel, //物件: 新增﹑異動 對應的資料
      actionObj, //物件: 動作物件
      m_queryObjData, //method: 查詢資料
      saveIt, //method: 新增﹑異動儲存
      curdAction, //method: 新增﹑異動 顯示lightbox
      deleteData //method: 刪除
    };
  }
};
</script>

<style></style>
