<%@ Page Language="C#" %>

<script runat="server">

	// Insert page code here
</script>

<html>
<head>
	<title>environment variables</title>
	<style>
		.whitecell {
			font-size: xx-small;
			background: white;
			font-family: Verdana, Arial, Helvetica, sans-serif;
		}
	</style>
</head>
<body bgcolor="lightseagreen">
	<center>
		<p>
			<table bordercolor="black" cellspacing="0" cellpadding="3" width="95%" border="1">
				<tbody>
					<tr class="whitecell">
						<th>
							Server Variable
						</th>
						<th>
							Value
						</th>
					</tr>
					<%
						foreach (string strKey in Request.ServerVariables) {
							if (strKey != "") {
								Response.Write("<TR class=whitecell><TD valign=top>" + strKey + "</TD>");
								Response.Write("<TD valign=top>" + Request.ServerVariables[strKey] + "&nbsp;</TD>");
								Response.Write("</TR>");
							}
						}
					%>
				</tbody>
			</table>
		</p>
	</center>
</body>
</html>