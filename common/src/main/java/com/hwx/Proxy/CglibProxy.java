package com.hwx.Proxy;

import net.sf.cglib.proxy.*;

import java.lang.reflect.Method;
import java.util.List;

public class CglibProxy  implements MethodInterceptor,CallbackFilter {
    private Enhancer enhancer = new Enhancer();


    public Object intercept(Object obj, Method method, Object[] args, MethodProxy proxy) throws Throwable{
        Object result =  proxy.invokeSuper(obj,args);
        return result;
    }

    public int accept(Method method) {
        if ("select".equals(method.getName())) {
            return 0;
        }
        return 1;
    }

    public Object getProxy(Class clazz){
        enhancer.setSuperclass(clazz);
        enhancer.setCallback(this);
        return enhancer.create();
    }

    public Object getProxy(Class clazz, List<Callback> callbacks){
        //callbacks.add(0,this);
        callbacks.add(NoOp.INSTANCE);//表示一个空Callback
        enhancer.setSuperclass(clazz);

        enhancer.setCallbacks(callbacks.toArray(new Callback[callbacks.size()]));
        enhancer.setCallbackFilter(this);
        enhancer.setInterceptDuringConstruction(false);//方法设置为false即可，默认为true，即构造函数中调用方法也是会拦截的。
        return enhancer.create();

    }



}
