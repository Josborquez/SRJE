import{i as u,_ as S,o as s,c as i,b as n,d as c,a as t,k as v,t as r,f as x,w as m,G as B,g as h}from"./index-zTRHdh1W.js";import{T as C}from"./trash-2-BWfXGZ_I.js";/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const D=u("cloud-upload",[["path",{d:"M12 13v8",key:"1l5pq0"}],["path",{d:"M4 14.899A7 7 0 1 1 15.71 8h1.79a4.5 4.5 0 0 1 2.5 8.242",key:"1pljnt"}],["path",{d:"m8 17 4-4 4 4",key:"1quai1"}]]);/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const M=u("file-check",[["path",{d:"M6 22a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h8a2.4 2.4 0 0 1 1.704.706l3.588 3.588A2.4 2.4 0 0 1 20 8v12a2 2 0 0 1-2 2z",key:"1oefj6"}],["path",{d:"M14 2v5a1 1 0 0 0 1 1h5",key:"wfsgrz"}],["path",{d:"m9 15 2 2 4-4",key:"1grp1n"}]]);/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const V=u("folder-open",[["path",{d:"m6 14 1.5-2.9A2 2 0 0 1 9.24 10H20a2 2 0 0 1 1.94 2.5l-1.54 6a2 2 0 0 1-1.95 1.5H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h3.9a2 2 0 0 1 1.69.9l.81 1.2a2 2 0 0 0 1.67.9H18a2 2 0 0 1 2 2v2",key:"usdka0"}]]),A={key:0,class:"upload-placeholder"},N={class:"upload-btn"},T=["accept"],q={key:0,class:"upload-formats"},w={key:1,class:"file-info"},H={class:"file-info-detail"},U={class:"file-name"},b={class:"file-size"},j={__name:"FileUpload",props:{accept:{type:String,default:""}},emits:["fileSelected"],setup(d,{emit:k}){const p=k,l=h(null),o=h(!1);function g(a){const e=a.target.files[0];e&&f(e)}function F(a){o.value=!1;const e=a.dataTransfer.files[0];e&&f(e)}function f(a){l.value=a,p("fileSelected",a)}function _(){l.value=null,p("fileSelected",null)}function z(a){return a<1024?a+" B":a<1048576?(a/1024).toFixed(1)+" KB":(a/1048576).toFixed(1)+" MB"}return(a,e)=>(s(),i("div",{class:B(["file-upload",{"drag-over":o.value}]),onDragover:e[0]||(e[0]=m(y=>o.value=!0,["prevent"])),onDragleave:e[1]||(e[1]=y=>o.value=!1),onDrop:m(F,["prevent"])},[l.value?(s(),i("div",w,[t("div",H,[n(c(M),{size:20,class:"file-info-icon"}),t("div",null,[t("span",U,r(l.value.name),1),t("span",b,r(z(l.value.size)),1)])]),t("button",{onClick:_,class:"btn-clear"},[n(c(C),{size:14}),e[5]||(e[5]=v(" Quitar ",-1))])])):(s(),i("div",A,[n(c(D),{size:40,class:"upload-icon"}),e[3]||(e[3]=t("p",{class:"upload-title"},"Arrastra un archivo aqui",-1)),e[4]||(e[4]=t("p",{class:"upload-subtitle"},"o seleccionalo desde tu computador",-1)),t("label",N,[n(c(V),{size:16}),e[2]||(e[2]=v(" Seleccionar archivo ",-1)),t("input",{type:"file",accept:d.accept,onChange:g,hidden:""},null,40,T)]),d.accept?(s(),i("small",q,"Formatos aceptados: "+r(d.accept),1)):x("",!0)]))],34))}},$=S(j,[["__scopeId","data-v-79d91804"]]);export{$ as F};
