<?xml version="1.0" encoding="iso-8859-1"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output method="html" indent="no" omit-xml-declaration="yes"/>


<xsl:template match="/">
    <div class = "FAQMasterRed">
      <ol class = "FAQMasterRoot ">
      <xsl:for-each select = "/root/faqs/faq">
          <li class = "faqRoot">
              <a class = "faqQuestion">
                  <xsl:attribute name="href">
                      <xsl:value-of select="relPath"/>
                  </xsl:attribute>
                  <xsl:value-of select="question" disable-output-escaping="yes"/>
              </a>
              <div class = "faqAnswer">
                  <xsl:if test = "opened='true'">
                      <xsl:attribute name = "style">display: block;</xsl:attribute>
                  </xsl:if>
                  <xsl:value-of select="answer" disable-output-escaping="yes"/>
              </div>
          </li>
      </xsl:for-each>
      </ol>
    </div>
    
    <script type = "text/javascript">

        avt_jQuery_1_3_2_FAQM(document).ready(function() {

            var _root = avt_jQuery_1_3_2_FAQM("#<xsl:value-of select="/root/rootElementId"/>");
            var _selItem = _root.find("a.faqQuestion[href="+ window.location.hash + "]").next(".faqAnswer");
            if (_selItem.length > 0) {
                _root.find(".faqAnswer").hide();
                _selItem.show();
            }
            
            _root.find(".faqQuestion").unbind("click");
            _root.find(".faqQuestion").click(function() {
                var _this = avt_jQuery_1_3_2_FAQM(this);
                var _answersVisible = _this.parents(".FAQMasterRoot:first").find(".faqAnswer:visible");
                if (_answersVisible.size() > 0) {
                    _this.parents(".FAQMasterRoot:first").find(".faqAnswer:visible").animate({ opacity: 0, height: 'hide' }, "fast", "linear", function() {
                        _this.parents(".faqRoot:first").find(".faqAnswer:first").animate({opacity: 1, height: 'show'}, "fast", "linear");
                    });
                } else {
                    _this.parents(".faqRoot:first").find(".faqAnswer:first").animate({ opacity: 1, height: 'show' }, "fast", "linear");
                }
                
            });
        });

    </script>
    
</xsl:template>

</xsl:stylesheet>