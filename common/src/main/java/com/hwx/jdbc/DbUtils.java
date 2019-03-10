package com.hwx.jdbc;

import java.io.InputStream;
import java.sql.*;
import java.util.*;

public  class DbUtils {
    private static Properties properties;
    static {
        try{
            InputStream is = DbUtils.class.getClassLoader().getResourceAsStream("dbconfig.properties");
            properties = new Properties();
            properties.load(is);
        }catch (Exception ex){
            throw new RuntimeException(ex);
        }
    }


    public static  Connection getConnection(){
        try {
            return DriverManager.getConnection(properties.getProperty("url"),properties.getProperty("username"),properties.getProperty("password"));
        } catch (Exception ex){
            throw new RuntimeException(ex);
        }
    }

    /**
     * 获得结果集行数
     * @param resultSet
     * @return
     */
    public static int gteResultSetRows(ResultSet resultSet){
        try{
            resultSet.last();
            return resultSet.getRow();
        }catch (SQLException ex){
            return 0;
        }
    }

    public static List<Map<String,Object>> excuteQuery(String sql,Object[] paras){
        List result = new ArrayList();
        Connection connection = null;
        PreparedStatement statement = null;
        ResultSet resultSet = null;

        try {
            //statement = connection.createStatement();
            statement = connection.prepareStatement(sql);
            for(int i =1;i<paras.length;i++){
                statement.setObject(i,paras[i-1]);
            }

            resultSet =  statement.executeQuery();
            ResultSetMetaData metaData = resultSet.getMetaData();
            int columnCount = metaData.getColumnCount();
            while (resultSet.next()){
                Map map = new HashMap();
                for(int i = 1;i <= columnCount;i++){
                    map.put(metaData.getColumnName(i),resultSet.getObject(i));
                }
                result.add(map);
            }
        }
        catch (Exception ex){
            throw  new RuntimeException(ex);
        }
        finally {
            try {
                if(resultSet!=null) resultSet.close();
                if(statement!=null) statement.close();
                if(connection!=null) connection.close();
            }
            catch (SQLException ex){
            }
        }
        return result;
    }
}
