import { useEffect, useState } from "react";
import { getUserDetails } from "../api/auth";
import styles from "./UserDetails.module.css";

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
    <div className={styles.detailsContainer}>
      <h1>User Details</h1>
      <p>
        <strong>First Name:</strong> {user.firstName}
      </p>
      <p>
        <strong>Last Name:</strong> {user.lastName}
      </p>
      <p>
        <strong>Email:</strong> {user.email}
      </p>
    </div>
  );
};

export default UserDetails;