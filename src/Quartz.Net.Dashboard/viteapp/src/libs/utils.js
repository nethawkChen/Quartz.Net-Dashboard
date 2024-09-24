// export function dateISOFormat(dateValue) {
//   const date = new Date(+new Date(dateValue) + 8 * 3600 * 1000);
//   return date.toISOString().slice(0, 10);
// }

export const dateISOFormat = dateValue => {
  const date = new Date(+new Date(dateValue) + 8 * 3600 * 1000);
  return date.toISOString().slice(0, 10);
}

// 格式化日期時間為 yyyy/MM/dd HH:mm:ss
export const datetimeISOFormat = dateValue => {
  // const date = new Date(+new Date(dateValue) + 24 * 3600 * 1000); // 原本的時間加 24 小時
  const date = new Date(dateValue);
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  const hours = String(date.getHours()).padStart(2, '0');
  const minutes = String(date.getMinutes()).padStart(2, '0');
  const seconds = String(date.getSeconds()).padStart(2, '0');
  return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
}