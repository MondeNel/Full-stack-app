import { useEffect, useState } from "react";
import { getUserDetails } from "../api/auth";
import { Container, Paper, Typography, Box } from "@mui/material";

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
      <Paper elevation={3} sx={{ padding: 4, mt: 8 }}>
        <Typography variant="h4" align="center" gutterBottom>
          User Details
        </Typography>
        <Box sx={{ mt: 2 }}>
          <Typography variant="body1"><strong>First Name:</strong> {user.firstName}</Typography>
          <Typography variant="body1"><strong>Last Name:</strong> {user.lastName}</Typography>
          <Typography variant="body1"><strong>Email:</strong> {user.email}</Typography>
        </Box>
      </Paper>
    </Container>
  );
};

export default UserDetails;