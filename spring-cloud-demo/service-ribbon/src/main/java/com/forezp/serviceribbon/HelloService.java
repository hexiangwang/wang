package com.forezp.serviceribbon;

import com.netflix.hystrix.contrib.javanica.annotation.HystrixCommand;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import org.springframework.web.client.RestTemplate;

@Service
public class HelloService {

    @Autowired
    RestTemplate restTemplate;

    @HystrixCommand(fallbackMethod = "hiError")
    public String hiService(String name) {
//        try {
//            Thread.sleep(60001);
//        }catch (Exception ex){
//            System.out.println(ex.getMessage());
//        }
        String re= restTemplate.getForObject("http://SERVICE-HI/hi?name=" + name, String.class);
        return re;
    }

    public String hiError(String name) {
        return "hi,"+name+",sorry,error!";
    }

}
