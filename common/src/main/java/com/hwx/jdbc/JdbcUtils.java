package com.hwx.jdbc;

import javax.sql.DataSource;
import java.sql.Connection;
import java.sql.SQLException;

public class JdbcUtils {
    private static DataSource dataSource ;
    private static ThreadLocal<Connection> tl = new ThreadLocal<Connection>();//事务专用连接

    public static Connection getConnection() throws SQLException {
        Connection tranConn = tl.get();
        //不为null，说明开启了事务
        if(tranConn!=null) return tranConn;
        return dataSource.getConnection();
    }

    public static DataSource getDataSource(){
        return dataSource;
    }

    public static void beginTransaction() throws SQLException{
        Connection tranConn = tl.get();
        if(tranConn!=null) throw new SQLException("已经开启了事务，不要重复开启！");
        tranConn = getConnection();
        tranConn.setAutoCommit(false);
        tl.set(tranConn);
    }

    public static void commitTransaction() throws SQLException{
        Connection tranConn = tl.get();
        if(tranConn==null) throw new SQLException("事务还没有开启，不能提交！");
        tranConn.commit();
        tranConn.close();
        tl.remove();
    }
    public static void rollbackTransaction() throws SQLException{
        Connection tranConn = tl.get();
        if(tranConn==null) throw new SQLException("事务还没有开启，不能回滚！");
        tranConn.rollback();
        tranConn.close();
        tl.remove();
    }

    public static void releaseConnection(Connection connection) throws SQLException{
        Connection tranConn = tl.get();
        if(connection!=tranConn) connection.close();
    }
}
