import { useState } from "react";
import { registerUser } from "../api/auth";
import { TextField, Button, Container, Typography, Box, Paper } from "@mui/material";

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
    <Container maxWidth="sm">
      <Paper elevation={3} sx={{ padding: 4, mt: 8 }}>
        <Typography variant="h4" align="center" gutterBottom>
          Create Account
        </Typography>
        <Box component="form" onSubmit={handleSubmit} sx={{ mt: 2, display: "flex", flexDirection: "column", gap: 2 }}>
          <TextField label="First Name" name="firstName" onChange={handleChange} fullWidth required />
          <TextField label="Last Name" name="lastName" onChange={handleChange} fullWidth required />
          <TextField label="Email" name="email" type="email" onChange={handleChange} fullWidth required />
          <TextField label="Password" name="password" type="password" onChange={handleChange} fullWidth required />
          <Button type="submit" variant="contained" color="primary" size="large">
            Register
          </Button>
        </Box>
      </Paper>
    </Container>
  );
};

export default Register;