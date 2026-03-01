import { useState } from "react";
import { registerUser } from "../api/auth";
import styles from "./Register.module.css";

const Register = () => {
  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
  });

  const handleChange = (e) =>
    setForm({ ...form, [e.target.name]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await registerUser(form);
      alert("Registration successful!");
    } catch (err) {
      alert("Error: " + err.response?.data?.message || err.message);
    }
  };

  return (
    <form onSubmit={handleSubmit} className={styles.formContainer}>
      <h1>Register</h1>
      <input
        className={styles.inputField}
        name="firstName"
        placeholder="First Name"
        onChange={handleChange}
      />
      <input
        className={styles.inputField}
        name="lastName"
        placeholder="Last Name"
        onChange={handleChange}
      />
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
        Register
      </button>
    </form>
  );
};

export default Register;