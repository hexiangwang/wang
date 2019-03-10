package com.hwx.jdbc;

import org.junit.Test;

import java.sql.*;
import java.util.Properties;

import static org.junit.Assert.*;

public class DbUtilsTest {

    @Test
    public void getConnection() throws SQLException {
        Connection connection;
        Statement statement;
        ResultSet resultSet;
        connection = DriverManager.getConnection("","","");
        statement = connection.createStatement();
        resultSet = statement.executeQuery("");
    }
}