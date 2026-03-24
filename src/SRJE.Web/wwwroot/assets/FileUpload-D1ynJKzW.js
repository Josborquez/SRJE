import{i as c,_ as z,o as s,c as i,b as n,d,a as t,k as v,t as p,f as S,w as h,B as V,g as m}from"./index-ByRvqtgS.js";/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const B=c("cloud-upload",[["path",{d:"M12 13v8",key:"1l5pq0"}],["path",{d:"M4 14.899A7 7 0 1 1 15.71 8h1.79a4.5 4.5 0 0 1 2.5 8.242",key:"1pljnt"}],["path",{d:"m8 17 4-4 4 4",key:"1quai1"}]]);/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const x=c("file-check",[["path",{d:"M6 22a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h8a2.4 2.4 0 0 1 1.704.706l3.588 3.588A2.4 2.4 0 0 1 20 8v12a2 2 0 0 1-2 2z",key:"1oefj6"}],["path",{d:"M14 2v5a1 1 0 0 0 1 1h5",key:"wfsgrz"}],["path",{d:"m9 15 2 2 4-4",key:"1grp1n"}]]);/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const C=c("folder-open",[["path",{d:"m6 14 1.5-2.9A2 2 0 0 1 9.24 10H20a2 2 0 0 1 1.94 2.5l-1.54 6a2 2 0 0 1-1.95 1.5H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h3.9a2 2 0 0 1 1.69.9l.81 1.2a2 2 0 0 0 1.67.9H18a2 2 0 0 1 2 2v2",key:"usdka0"}]]);/**
 * @license lucide-vue-next v0.577.0 - ISC
 *
 * This source code is licensed under the ISC license.
 * See the LICENSE file in the root directory of this source tree.
 */const D=c("trash-2",[["path",{d:"M10 11v6",key:"nco0om"}],["path",{d:"M14 11v6",key:"outv1u"}],["path",{d:"M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6",key:"miytrc"}],["path",{d:"M3 6h18",key:"d0wm0j"}],["path",{d:"M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2",key:"e791ji"}]]),j={key:0,class:"upload-placeholder"},w={class:"upload-btn"},A=["accept"],H={key:0,class:"upload-formats"},N={key:1,class:"file-info"},q={class:"file-info-detail"},T={class:"file-name"},U={class:"file-size"},b={__name:"FileUpload",props:{accept:{type:String,default:""}},emits:["fileSelected"],setup(r,{emit:k}){const u=k,l=m(null),o=m(!1);function y(a){const e=a.target.files[0];e&&f(e)}function g(a){o.value=!1;const e=a.dataTransfer.files[0];e&&f(e)}function f(a){l.value=a,u("fileSelected",a)}function F(){l.value=null,u("fileSelected",null)}function M(a){return a<1024?a+" B":a<1048576?(a/1024).toFixed(1)+" KB":(a/1048576).toFixed(1)+" MB"}return(a,e)=>(s(),i("div",{class:V(["file-upload",{"drag-over":o.value}]),onDragover:e[0]||(e[0]=h(_=>o.value=!0,["prevent"])),onDragleave:e[1]||(e[1]=_=>o.value=!1),onDrop:h(g,["prevent"])},[l.value?(s(),i("div",N,[t("div",q,[n(d(x),{size:20,class:"file-info-icon"}),t("div",null,[t("span",T,p(l.value.name),1),t("span",U,p(M(l.value.size)),1)])]),t("button",{onClick:F,class:"btn-clear"},[n(d(D),{size:14}),e[5]||(e[5]=v(" Quitar ",-1))])])):(s(),i("div",j,[n(d(B),{size:40,class:"upload-icon"}),e[3]||(e[3]=t("p",{class:"upload-title"},"Arrastra un archivo aqui",-1)),e[4]||(e[4]=t("p",{class:"upload-subtitle"},"o seleccionalo desde tu computador",-1)),t("label",w,[n(d(C),{size:16}),e[2]||(e[2]=v(" Seleccionar archivo ",-1)),t("input",{type:"file",accept:r.accept,onChange:y,hidden:""},null,40,A)]),r.accept?(s(),i("small",H,"Formatos aceptados: "+p(r.accept),1)):S("",!0)]))],34))}},O=z(b,[["__scopeId","data-v-79d91804"]]);export{O as F};
