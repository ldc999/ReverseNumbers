import request from './request'

export default {
  generateRandom(params) {
    return request.post('/generate/random', params)
  },
  highlight(params) {
    return request.post('/generate/highlight', params)
  }
}
