package com.hwx.common.util;

import com.hwx.jdbc.Person;
import com.hwx.jdbc.Student;
import org.junit.Test;

import static org.junit.Assert.*;

public class EntityUtilsTest {

    @Test
    public void copyProperties() {
        Student s = new Student();
        s.setName("xiangwang");
        s.setAge(18);

        Person p = new Person();
        EntityUtils.copyProperties(s,p,false);

        System.out.println(p.getName());
    }
}