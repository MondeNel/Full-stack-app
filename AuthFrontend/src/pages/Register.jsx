import { useState } from "react";
import { registerUser } from "../api/auth";
import {
  TextField,
  Button,
  Container,
  Typography,
  Box,
  Paper,
  InputAdornment,
  IconButton
} from "@mui/material";
import AccountCircle from "@mui/icons-material/AccountCircle";
import EmailIcon from "@mui/icons-material/Email";
import LockIcon from "@mui/icons-material/Lock";
import Visibility from "@mui/icons-material/Visibility";
import VisibilityOff from "@mui/icons-material/VisibilityOff";

const Register = () => {
  const [form, setForm] = useState({ firstName: "", lastName: "", email: "", password: "" });
  const [showPassword, setShowPassword] = useState(false);

  const handleChange = (e) => setForm({ ...form, [e.target.name]: e.target.value });
  const handleTogglePassword = () => setShowPassword(!showPassword);

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
      <Paper elevation={5} sx={{ padding: 4, mt: 8 }}>
        <Typography variant="h4" align="center" gutterBottom>Create Account</Typography>
        <Box component="form" onSubmit={handleSubmit} sx={{ mt: 2, display: "flex", flexDirection: "column", gap: 2 }}>
          <TextField
            label="First Name"
            name="firstName"
            onChange={handleChange}
            fullWidth
            required
            InputProps={{ startAdornment: <InputAdornment position="start"><AccountCircle /></InputAdornment> }}
          />
          <TextField
            label="Last Name"
            name="lastName"
            onChange={handleChange}
            fullWidth
            required
            InputProps={{ startAdornment: <InputAdornment position="start"><AccountCircle /></InputAdornment> }}
          />
          <TextField
            label="Email"
            name="email"
            type="email"
            onChange={handleChange}
            fullWidth
            required
            InputProps={{ startAdornment: <InputAdornment position="start"><EmailIcon /></InputAdornment> }}
          />
          <TextField
            label="Password"
            name="password"
            type={showPassword ? "text" : "password"}
            onChange={handleChange}
            fullWidth
            required
            InputProps={{
              startAdornment: <InputAdornment position="start"><LockIcon /></InputAdornment>,
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton onClick={handleTogglePassword} edge="end">
                    {showPassword ? <VisibilityOff /> : <Visibility />}
                  </IconButton>
                </InputAdornment>
              )
            }}
          />
          <Button type="submit" variant="contained" color="primary" size="large">Register</Button>
        </Box>
      </Paper>
    </Container>
  );
};

export default Register;