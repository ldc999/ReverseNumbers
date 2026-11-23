import axios from 'axios'

const request = axios.create({
  baseURL: '/api',
  timeout: 3000000
})

request.interceptors.response.use(
  response => {
    const data = response.data
    if (data.code !== 0) {
      alert(data.message || '请求失败')
      return Promise.reject(new Error(data.message))
    }
    return data
  },
  error => {
    alert('网络错误：' + error.message)
    return Promise.reject(error)
  }
)

export default request
