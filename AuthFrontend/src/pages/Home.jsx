import { Container, Paper, Typography, Button, Box } from "@mui/material";
import LoginIcon from "@mui/icons-material/Login";
import PersonAddIcon from "@mui/icons-material/PersonAdd";
import { useNavigate } from "react-router-dom";

const Home = () => {
  const navigate = useNavigate();

  return (
    <Container maxWidth="sm">
      <Paper elevation={5} sx={{ padding: 6, mt: 12, textAlign: "center" }}>
        <Typography variant="h3" gutterBottom>
          Welcome to MyApp
        </Typography>
        <Typography variant="subtitle1" gutterBottom>
          Please login or register to continue
        </Typography>

        <Box sx={{ mt: 4, display: "flex", justifyContent: "center", gap: 3 }}>
          <Button
            variant="contained"
            color="primary"
            startIcon={<LoginIcon />}
            onClick={() => navigate("/login")}
            size="large"
          >
            Login
          </Button>
          <Button
            variant="outlined"
            color="primary"
            startIcon={<PersonAddIcon />}
            onClick={() => navigate("/register")}
            size="large"
          >
            Register
          </Button>
        </Box>
      </Paper>
    </Container>
  );
};

export default Home;