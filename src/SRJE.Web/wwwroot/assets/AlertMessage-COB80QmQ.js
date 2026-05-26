import{i as o,_ as p,G as f,q as y,o as r,x as i,j as h,c as k,E as g,R as v,a as n,t as C,b,d as x,f as _,T as M,g as T,D as z}from"./index-BNwbpPN2.js";/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const A=o("circle-check-big",[["path",{d:"M21.801 10A10 10 0 1 1 17 3.335",key:"yps3ct"}],["path",{d:"m9 11 3 3L22 4",key:"1pflzl"}]]);/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const B=o("circle-x",[["circle",{cx:"12",cy:"12",r:"10",key:"1mglay"}],["path",{d:"m15 9-6 6",key:"1uzhvr"}],["path",{d:"m9 9 6 6",key:"z0biqf"}]]);/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const w=o("info",[["circle",{cx:"12",cy:"12",r:"10",key:"1mglay"}],["path",{d:"M12 16v-4",key:"1dtifu"}],["path",{d:"M12 8h.01",key:"e9boi3"}]]);/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const I=o("triangle-alert",[["path",{d:"m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3",key:"wmoenq"}],["path",{d:"M12 9v4",key:"juzpu7"}],["path",{d:"M12 17h.01",key:"p32p05"}]]);/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const N=o("x",[["path",{d:"M18 6 6 18",key:"1bl5f8"}],["path",{d:"m6 6 12 12",key:"d8bk6v"}]]),q={class:"alert-text"},D={__name:"AlertMessage",props:{message:{type:String,default:""},type:{type:String,default:"info",validator:a=>["success","error","warning","info"].includes(a)},duration:{type:Number,default:5e3},autoClose:{type:Boolean,default:!0}},emits:["close"],setup(a,{emit:u}){const t=a,d=u,c=T(!1);let e=null;const m=z(()=>{const s={success:A,error:B,warning:I,info:w};return s[t.type]||s.info});f(()=>t.message,s=>{s&&(c.value=!0,e&&clearTimeout(e),t.autoClose&&t.duration>0&&(e=setTimeout(l,t.duration)))},{immediate:!0}),y(()=>{e&&clearTimeout(e)});function l(){c.value=!1,e&&clearTimeout(e),d("close")}return(s,S)=>(r(),i(M,{name:"alert-fade"},{default:h(()=>[c.value?(r(),k("div",{key:0,class:g(["alert",`alert-${a.type}`]),role:"alert"},[(r(),i(v(m.value),{size:18,class:"alert-icon"})),n("span",q,C(a.message),1),n("button",{class:"alert-close",onClick:l,"aria-label":"Cerrar"},[b(x(N),{size:16})])],2)):_("",!0)]),_:1}))}},j=p(D,[["__scopeId","data-v-b1824684"]]);export{j as A,B as C,w as I,I as T,N as X,A as a};
