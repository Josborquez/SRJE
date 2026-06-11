import{i as c,_ as f,H as p,q as y,o as r,x as i,j as g,c as k,G as h,W as C,a as n,t as v,b,d as _,f as x,T,g as z,E as B,X as A,P as w}from"./index-CD4b6Z7x.js";/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const M=c("circle-check-big",[["path",{d:"M21.801 10A10 10 0 1 1 17 3.335",key:"yps3ct"}],["path",{d:"m9 11 3 3L22 4",key:"1pflzl"}]]);/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const N=c("circle-x",[["circle",{cx:"12",cy:"12",r:"10",key:"1mglay"}],["path",{d:"m15 9-6 6",key:"1uzhvr"}],["path",{d:"m9 9 6 6",key:"z0biqf"}]]);/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const I=c("x",[["path",{d:"M18 6 6 18",key:"1bl5f8"}],["path",{d:"m6 6 12 12",key:"d8bk6v"}]]),S={class:"alert-text"},V={__name:"AlertMessage",props:{message:{type:String,default:""},type:{type:String,default:"info",validator:a=>["success","error","warning","info"].includes(a)},duration:{type:Number,default:5e3},autoClose:{type:Boolean,default:!0}},emits:["close"],setup(a,{emit:u}){const t=a,m=u,o=z(!1);let e=null;const d=B(()=>{const s={success:M,error:N,warning:w,info:A};return s[t.type]||s.info});p(()=>t.message,s=>{s&&(o.value=!0,e&&clearTimeout(e),t.autoClose&&t.duration>0&&(e=setTimeout(l,t.duration)))},{immediate:!0}),y(()=>{e&&clearTimeout(e)});function l(){o.value=!1,e&&clearTimeout(e),m("close")}return(s,X)=>(r(),i(T,{name:"alert-fade"},{default:g(()=>[o.value?(r(),k("div",{key:0,class:h(["alert",`alert-${a.type}`]),role:"alert"},[(r(),i(C(d.value),{size:18,class:"alert-icon"})),n("span",S,v(a.message),1),n("button",{class:"alert-close",onClick:l,"aria-label":"Cerrar"},[b(_(I),{size:16})])],2)):x("",!0)]),_:1}))}},D=f(V,[["__scopeId","data-v-b1824684"]]);export{D as A,N as C,I as X,M as a};
