package com.hwx.common.util;

import org.junit.Test;

import static org.junit.Assert.*;

public class JdkProxyUtilsTest {

    @Test
    public void newProxyInstance() throws Exception {

       CglibProxy proxy = new CglibProxy();
        //JdkProxy proxy = new JdkProxy();
        //通过生成子类的方式创建代理类
        IUserManager um = (IUserManager)proxy.getProxy(UserManager.class);
        um.function();



    }
}