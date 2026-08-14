{{- define "enterprisehub.fullname" -}}
{{ .Release.Name }}
{{- end -}}

{{- define "enterprisehub.labels" -}}
app.kubernetes.io/part-of: enterprisehub
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end -}}
