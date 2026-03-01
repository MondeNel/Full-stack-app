import axios from "axios";

const API_URL = "http://localhost:5277/api/auth"; // Updated to correct backend URL and endpoint

/**
 * Register a new user
 * @param {Object} data - { firstName, lastName, email, password }
 */
export const registerUser = (data) => {
  return axios.post(`${API_URL}/register`, data);
};

/**
 * Login a user
 * @param {Object} data - { email, password }
 */
export const loginUser = (data) => {
  return axios.post(`${API_URL}/login`, data);
};

/**
 * Get current user details (requires token)
 * @param {string} token - JWT token
 */
export const getUserDetails = (token) => {
  return axios.get(`${API_URL}/me`, {
    headers: { Authorization: `Bearer ${token}` },
  });
};