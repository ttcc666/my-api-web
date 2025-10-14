import axios from 'axios'
import { apiConfig } from '@/config'

const apiClient = axios.create({
  baseURL: apiConfig.baseURL,
  timeout: apiConfig.timeout,
})

export default apiClient
