import{i as c,_ as p,D as f,o as r,q as i,j as y,c as h,B as k,Q as g,a as n,t as v,b as C,d as b,f as _,T as x,g as M,A as z}from"./index-D-oJ99K_.js";/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const A=c("circle-check-big",[["path",{d:"M21.801 10A10 10 0 1 1 17 3.335",key:"yps3ct"}],["path",{d:"m9 11 3 3L22 4",key:"1pflzl"}]]);/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const B=c("circle-x",[["circle",{cx:"12",cy:"12",r:"10",key:"1mglay"}],["path",{d:"m15 9-6 6",key:"1uzhvr"}],["path",{d:"m9 9 6 6",key:"z0biqf"}]]);/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const T=c("info",[["circle",{cx:"12",cy:"12",r:"10",key:"1mglay"}],["path",{d:"M12 16v-4",key:"1dtifu"}],["path",{d:"M12 8h.01",key:"e9boi3"}]]);/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const w=c("triangle-alert",[["path",{d:"m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3",key:"wmoenq"}],["path",{d:"M12 9v4",key:"juzpu7"}],["path",{d:"M12 17h.01",key:"p32p05"}]]);/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const I=c("x",[["path",{d:"M18 6 6 18",key:"1bl5f8"}],["path",{d:"m6 6 12 12",key:"d8bk6v"}]]),N={class:"alert-text"},q={__name:"AlertMessage",props:{message:{type:String,default:""},type:{type:String,default:"info",validator:e=>["success","error","warning","info"].includes(e)},duration:{type:Number,default:5e3},autoClose:{type:Boolean,default:!0}},emits:["close"],setup(e,{emit:u}){const a=e,d=u,o=M(!1);let t=null;const m=z(()=>{const s={success:A,error:B,warning:w,info:T};return s[a.type]||s.info});f(()=>a.message,s=>{s&&(o.value=!0,t&&clearTimeout(t),a.autoClose&&a.duration>0&&(t=setTimeout(l,a.duration)))},{immediate:!0});function l(){o.value=!1,t&&clearTimeout(t),d("close")}return(s,D)=>(r(),i(x,{name:"alert-fade"},{default:y(()=>[o.value?(r(),h("div",{key:0,class:k(["alert",`alert-${e.type}`]),role:"alert"},[(r(),i(g(m.value),{size:18,class:"alert-icon"})),n("span",N,v(e.message),1),n("button",{class:"alert-close",onClick:l,"aria-label":"Cerrar"},[C(b(I),{size:16})])],2)):_("",!0)]),_:1}))}},V=p(q,[["__scopeId","data-v-f7eff478"]]);export{V as A,B as C,T as I,w as T,I as X,A as a};
