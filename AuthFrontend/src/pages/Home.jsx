import styles from "./Home.module.css";

const Home = () => (
  <div className={styles.homeContainer}>
    <h1>Welcome to the App</h1>
    <p>
      <a href="/login">Login</a> | <a href="/register">Register</a>
    </p>
  </div>
);

export default Home;