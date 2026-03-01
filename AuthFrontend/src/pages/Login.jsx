import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { loginUser } from "../api/auth";
import styles from "./Login.module.css";

const Login = () => {
  const [form, setForm] = useState({ email: "", password: "" });
  const navigate = useNavigate();

  const handleChange = (e) =>
    setForm({ ...form, [e.target.name]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const res = await loginUser(form);
      localStorage.setItem("token", res.data.token);
      navigate("/user");
    } catch (err) {
      alert("Login failed: " + err.response?.data?.message || err.message);
    }
  };

  return (
    <form onSubmit={handleSubmit} className={styles.formContainer}>
      <h1>Login</h1>
      <input
        className={styles.inputField}
        name="email"
        placeholder="Email"
        onChange={handleChange}
      />
      <input
        className={styles.inputField}
        name="password"
        type="password"
        placeholder="Password"
        onChange={handleChange}
      />
      <button className={styles.submitButton} type="submit">
        Login
      </button>
    </form>
  );
};

export default Login;