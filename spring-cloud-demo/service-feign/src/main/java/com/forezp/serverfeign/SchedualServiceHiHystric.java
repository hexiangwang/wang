package com.forezp.serverfeign;

import org.springframework.stereotype.Component;

@Component
public class SchedualServiceHiHystric implements SchedualeServiceHi {
    @Override
    public String sayHiFromClientOne(String name) {
        return "sorry "+name;
    }
}
