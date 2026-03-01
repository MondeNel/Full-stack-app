import { useEffect, useState } from "react";
import { getUserDetails } from "../api/auth";
import { Container, Paper, Typography, Box } from "@mui/material";
import AccountCircle from "@mui/icons-material/AccountCircle";
import EmailIcon from "@mui/icons-material/Email";

const UserDetails = () => {
  const [user, setUser] = useState(null);

  useEffect(() => {
    const token = localStorage.getItem("token");
    getUserDetails(token)
      .then((res) => setUser(res.data))
      .catch((err) => alert(err));
  }, []);

  if (!user) return <p>Loading...</p>;

  return (
    <Container maxWidth="sm">
      <Paper elevation={5} sx={{ padding: 4, mt: 8 }}>
        <Typography variant="h4" align="center" gutterBottom>User Details</Typography>
        <Box sx={{ mt: 2, display: "flex", flexDirection: "column", gap: 1 }}>
          <Typography variant="body1"><AccountCircle /> <strong>First Name:</strong> {user.firstName}</Typography>
          <Typography variant="body1"><AccountCircle /> <strong>Last Name:</strong> {user.lastName}</Typography>
          <Typography variant="body1"><EmailIcon /> <strong>Email:</strong> {user.email}</Typography>
        </Box>
      </Paper>
    </Container>
  );
};

export default UserDetails;