import axios from "axios";

const API_URL = "http://localhost:5277"; // Connecting to backend URL

/**
 * @param {Object} userData
 * @returns Promise
 */
export const registerUser = (userData) => {
  return axios.post(`${API_URL}/user/register`, userData);
};

/**
 * @param {Object} credentials
 * @returns Promise
 */
export const loginUser = (credentials) => {
  return axios.post(`${API_URL}/user/login`, credentials);
};

/**
 * @param {string} token
 * @returns Promise
 */
export const getUserDetails = (token) => {
  return axios.get(`${API_URL}/user/details`, {
    headers: { Authorization: `Bearer ${token}` },
  });
};